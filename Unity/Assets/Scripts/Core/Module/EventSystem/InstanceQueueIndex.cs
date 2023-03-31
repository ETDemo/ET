namespace ET
{
    //LCM: 定义事件队列 类型（顺序无关）
    public enum InstanceQueueIndex
    {
        None = -1,
        Update,
        LateUpdate,
        Load,
        Max,
    }
}