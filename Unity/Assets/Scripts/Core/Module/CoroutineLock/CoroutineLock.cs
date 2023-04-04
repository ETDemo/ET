using System;

namespace ET
{
    public class CoroutineLock: IDisposable
    {
        private int type;
        private long key;
        //LCM: 协程锁队列未间断的情况下（间断了就又重1开始），总共执行了多少次 协程操作
        //LCM: 注意不是队列里排队了多少 排队任务，而是 返回了多少次锁，即轮了多少次需要锁的协程操作
        //LCM: 协程锁是 排队任务 结束时才创建的，即当队列存在排队时，一个协程锁释放，另一个协程锁创建，并使level+1
        //LCM: 可能就是想给个警告吧，觉得应该检查 排队数量，这里逻辑不太对
        private int level;  
        
        public static CoroutineLock Create(int type, long k, int count)
        {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>();
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }
        
        public void Dispose()
        {
            //LCM: 标记为释放状态 （实际的释放操作在下一帧）
            CoroutineLockComponent.Instance.RunNextCoroutine(this.type, this.key, this.level + 1);
            
            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}