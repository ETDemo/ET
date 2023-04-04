using System;
using System.Collections.Generic;

namespace ET
{
    /*
     * LCM:
     * 调用 await CoroutineLockComponent.Instance.Wait()的时候，并不会立即创建一个协程锁
     * 而是创建 WaitCoroutineLock (实际上是 ETTask<CoroutineLock>）来排队
     * 排队结束后，才会创建 协程锁 CoroutineLock 作为ETTask<CoroutineLock>返回值
     * 得到协程锁，即开始执行此次协程操作了，操作完毕后 调用 CoroutineLock.dispose() 以标记该协程锁需要释放
     * 这一帧标记，下一帧才会真的执行释放操作，队列设置第一个未超时的 ETTask<CoroutineLock>的返回值
     * 。。。循环。。。
     */
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate
    {
        //LCM： type-Dic<key,queue>
        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>(CoroutineLockType.Max);
        //LCM: type-key-level
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>();

        public CoroutineLockComponent()
        {
            for (int i = 0; i < CoroutineLockType.Max; ++i)
            {
                CoroutineLockQueueType coroutineLockQueueType = new CoroutineLockQueueType(i);
                this.list.Add(coroutineLockQueueType);
            }
        }

        public override void Dispose()
        {
            this.list.Clear();
            this.nextFrameRun.Clear();
        }

        public void Update()
        {
            //LCM:上一帧记录，这一帧运行 （相对于上一帧记录那一刻来说，叫 nextFrame）
            //LCM:统一释放上一帧标记为释放的 协程锁
            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0)
            {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }
        }

        //LCM:这里只做记录，不立即运行，等下一帧运行
        //LCM:私自不要调用这个方法，这不是个 public 方法，由 CoroutineLock.dispose()调用
        public void RunNextCoroutine(int coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            this.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }
        
        //LCM:返回协程锁，当协程执行完毕后，手动调用dispose，即可释放协程锁
        //LCM:注意这里，协程锁的排队不能被打断，逻辑里是排完队后必须要返回一个协程锁（不然就重构），所以 ETCancelToken的判断要写在 被锁住的协程操作开头，这样才安全
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            return await coroutineLockQueueType.Wait(key, time);
        }
        
        private void Notify(int coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            coroutineLockQueueType.Notify(key, level);
        }
    }
}