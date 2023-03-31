using System.Collections.Generic;

namespace ET
{
    public enum TimerClass
    {
        None,
        //LCM:一次性定时器
        OnceTimer,
        //LCM:用于异步方法创建的定时器, 此时定时器参数为 ETTask
        OnceWaitTimer,
        //LCM:重复定时器
        RepeatedTimer,          
    }

    public class TimerAction
    {
        public static TimerAction Create(long id, TimerClass timerClass, long startTime, long time, int type, object obj)
        {
            TimerAction timerAction = ObjectPool.Instance.Fetch<TimerAction>();
            timerAction.Id = id;
            timerAction.TimerClass = timerClass;
            timerAction.StartTime = startTime;
            timerAction.Object = obj;
            timerAction.Time = time;
            timerAction.Type = type;
            return timerAction;
        }

        public long Id;
        
        public TimerClass TimerClass;

        //LCM：定时器的 参数，注意 装箱/拆箱
        public object Object;

        public long StartTime;

        public long Time;

        //LCM:这个Type其实是 InvokeType
        public int Type;
        
        public void Recycle()
        {
            this.Id = 0;
            this.Object = null;     
            this.StartTime = 0;
            this.Time = 0;
            this.TimerClass = TimerClass.None;
            this.Type = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }

    public struct TimerCallback
    {
        public object Args;
    }

    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate
    {
        //LCM：根据 key排序的字典   剩余延迟时间-timerID
        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        private readonly MultiMap<long, long> TimeId = new();

        private readonly Queue<long> timeOutTime = new(); 

        private readonly Queue<long> timeOutTimerIds = new();

        private readonly Dictionary<long, TimerAction> timerActions = new();

        private long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue;   

        private long GetId()
        {
            return ++this.idGenerator;
        }

        private static long GetNow()
        {
            return TimeHelper.ClientFrameTime();
        }

        //LCM:将所有超时的Timer，一起调用（有序）
        public void Update()
        {
            if (this.TimeId.Count == 0)
            {
                return;
            }

            long timeNow = GetNow();  //LCM:从这里可以看出，采用的是实际时间

            //LCM:在addTimer时，会将minTime设置为最小的那个时间
            if (timeNow < this.minTime)   
            {
                return;
            }

            foreach (KeyValuePair<long, List<long>> kv in this.TimeId)  
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    this.minTime = k;
                    break;
                }

                this.timeOutTime.Enqueue(k);
            }

            while (this.timeOutTime.Count > 0)
            {
                long time = this.timeOutTime.Dequeue();
                var list = this.TimeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    this.timeOutTimerIds.Enqueue(timerId);
                }
                this.TimeId.Remove(time);
            }

            while (this.timeOutTimerIds.Count > 0)
            {
                long timerId = this.timeOutTimerIds.Dequeue();

                if (!this.timerActions.Remove(timerId, out TimerAction timerAction))
                {
                    continue;
                }
                
                this.Run(timerAction);
            }
        }

        private void Run(TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.RepeatedTimer:
                {                    
                    long timeNow = GetNow();
                    timerAction.StartTime = timeNow;
                    this.AddTimer(timerAction);
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
            }
        }

        private void AddTimer(TimerAction timer)
        {
            long tillTime = timer.StartTime + timer.Time;
            this.TimeId.Add(tillTime, timer.Id);
            this.timerActions.Add(timer.Id, timer);
            if (tillTime < this.minTime)
            {
                this.minTime = tillTime;
            }
        }

        public bool Remove(ref long id)
        {
            long i = id;
            id = 0;
            return this.Remove(i);
        }

        private bool Remove(long id)
        {
            if (id == 0)
            {
                return false;
            }
            //LCM:只是在timerAction里移除了，TimeId集合没做处理（有点复杂），但是不影响，只是会遍历，不会触发
            if (!this.timerActions.Remove(id, out TimerAction timerAction))
            {
                return false;
            }
            timerAction.Recycle();
            return true;
        }

        public async ETTask WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            long timeNow = GetNow();
            if (timeNow >= tillTime)
            {
                return;
            }

            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        public async ETTask WaitFrameAsync(ETCancellationToken cancellationToken = null)
        {
            await this.WaitAsync(1, cancellationToken);
        }

        public async ETTask WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return;
            }

            long timeNow = GetNow();

            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        // LCM:WaitTillAsync 的 timerClass 为 onceWaitTimer , timerType 为 0，无法自己实现 ATimer，传参固定为 ETTask 
        // LCM:NewOnceTimer 的 timerClass 为 OnceTimer , 可以指定自定义 timerType, 自己实现 ATimer，参数自定义
        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long tillTime, int type, object args)
        {
            long timeNow = GetNow();
            if (tillTime < timeNow)
            {
                Log.Error($"new once time too small: {tillTime}");
            }

            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            this.AddTimer(timer);
            return timer.Id;
        }

        //LCM:每帧调用的定时器 ，理解为可自己注册取消的 Update
        public long NewFrameTimer(int type, object args)
        {
#if DOTNET
            return this.NewRepeatedTimerInner(100, type, args);
#else
            return this.NewRepeatedTimerInner(0, type, args);
#endif
        }

        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private long NewRepeatedTimerInner(long time, int type, object args)
        {
#if DOTNET
            if (time < 100)
            {
                throw new Exception($"repeated timer < 100, timerType: time: {time}");
            }
#endif
            
            long timeNow = GetNow();
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.RepeatedTimer, timeNow, time, type, args);
            //LCM:AddTimer就 加到 TimeID里了
            // 每帧执行的不用加到timerId中，防止遍历
            this.AddTimer(timer);
            return timer.Id;
        }

        public long NewRepeatedTimer(long time, int type, object args)
        {
            if (time < 100)
            {
                Log.Error($"time too small: {time}");
                return 0;
            }

            return this.NewRepeatedTimerInner(time, type, args);
        }
    }
}