using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace ET
{
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion
    {
        public static Action<Exception> ExceptionHandler;
        
        public static ETTaskCompleted CompletedTask
        {
            get
            {
                return new ETTaskCompleted();
            }
        }

        private static readonly Queue<ETTask> queue = new Queue<ETTask>();

        //LCM:使用对象池，必须要在await之后将对改 EtTask 的引用设置为null
        //LCM:因为当ETTask被放回池中的时候，其状态被设置为了Pending（这样从池中取出也默认是Pending状态）
        //LCM:所以操作池中的 EtTask 会导致报错 。
        //LCM:再次操作是指: await -> GetResult() 或 setResult()
        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask();
            }
            
            if (queue.Count == 0)
            {
                return new ETTask() {fromPool = true};    
            }
            return queue.Dequeue();
        }

        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            
            this.state = AwaiterStatus.Pending;
            this.callback = null;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(this);
        }

        private bool fromPool;
        private AwaiterStatus state;
        private object callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }
        
        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            //LCM: ETVoid.Coroutine() = Do Nothing
            InnerCoroutine().Coroutine();
        }

        [DebuggerHidden]
        public ETTask GetAwaiter()
        {
            return this;
        }

        
        public bool IsCompleted
        {
            [DebuggerHidden]
            get
            {
                return this.state != AwaiterStatus.Pending;
            }
        }

        //LCM:在c#开发者的解释里，UnsafeOnCompleted与OnCompleted的功能完全一致
        //LCM:但是区别在于 UnsafeOnCompleted 的期望是只由异步系统调用，OnCompleted的期望是开发者可以调用（虽然不会有人轻易调用）
        //LCM:因为是Public方法，所以需要添加特别特性才能实现限制（由编辑器的分析器来报错）
        //LCM:如果实现了 ICriticalNotifyCompletion ，那么异步系统会调用UnsafeOnCompleted，否则异步系统会调用OnCompleted
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                action?.Invoke();   //LCM:回调
                return;
            }

            this.callback = action;  //LCM:MoveNext
        }

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

        //LCM: async/await 系统调用，不得擅自调用，否则就重复调用了
        [DebuggerHidden]
        public void GetResult()
        {
            switch (this.state)
            {
                case AwaiterStatus.Succeeded:
                    this.Recycle();
                    break;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                    this.callback = null;
                    this.Recycle();
                    c?.Throw();
                    break;
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        //LCM: 只能调用一次
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Succeeded;

            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }

        //LCM: 只能调用一次
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Faulted;

            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }

    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T>: ICriticalNotifyCompletion
    {
        private static readonly Queue<ETTask<T>> queue = new Queue<ETTask<T>>();
        
        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask<T>();
            }
            
            if (queue.Count == 0)
            {
                return new ETTask<T>() { fromPool = true };    
            }
            return queue.Dequeue();
        }
        
        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            this.callback = null;
            this.value = default;
            this.state = AwaiterStatus.Pending;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(this);
        }

        private bool fromPool;
        private AwaiterStatus state;
        private T value;
        private object callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [DebuggerHidden]
        public ETTask<T> GetAwaiter()
        {
            return this;
        }

        [DebuggerHidden]
        public T GetResult()
        {
            switch (this.state)
            {
                case AwaiterStatus.Succeeded:
                    T v = this.value;
                    this.Recycle();
                    return v;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                    this.callback = null;
                    this.Recycle();
                    c?.Throw();
                    return default;
                default:
                    throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }


        public bool IsCompleted
        {
            [DebuggerHidden]
            get
            {
                return state != AwaiterStatus.Pending;
            }
        } 

        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            this.callback = action;
        }

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Succeeded;

            this.value = result;

            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }
        
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Faulted;

            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }
}