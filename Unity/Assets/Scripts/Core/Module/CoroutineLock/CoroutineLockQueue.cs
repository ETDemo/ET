using System;
using System.Collections.Generic;

namespace ET
{
    //LCM: 协程锁队列
    public class CoroutineLockQueue
    {
        private int type;
        private long key;
        
        public static CoroutineLockQueue Create(int type, long key)
        {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }

        private CoroutineLock currentCoroutineLock;
        
        //LCM: Queue<ETTask<CoroutineLock>> WaitCoroutineLock就是对ETTask<CoroutineLock>的一层包装
        //LCM: 正在排队中的 ETTask
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        public async ETTask<CoroutineLock> Wait(int time)
        {
            //LCM:队列中无排队情况，返回新创建的锁，用于这次异步操作
            if (this.currentCoroutineLock == null)
            {
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return this.currentCoroutineLock;
            }
            
            //LCM:先排队
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            this.queue.Enqueue(waitCoroutineLock);
            if (time > 0)       //LCM:超时处理，毫秒， <=0表示不限制时间
            {
                long tillTime = TimeHelper.ClientFrameTime() + time;
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            //LCM：排完队之后，返回一个新锁，用于这次异步操作
            this.currentCoroutineLock = await waitCoroutineLock.Wait();
            return this.currentCoroutineLock;
        }

        public void Notify(int level)
        {
            //LCM：一直出列，直到第一个可用的 waitTask
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                //LCM: 当前这个waitTask已经结束了，创建一个新的锁作为 这个 排队任务 的返回值，以进行新的异步操作
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level);

                waitCoroutineLock.SetResult(coroutineLock);
                break;
            }
        }

        public void Recycle()
        {
            this.queue.Clear();
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}