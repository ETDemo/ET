﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET
{
    public class EventSystem: Singleton<EventSystem>, ISingletonUpdate, ISingletonLateUpdate
    {
        //LCM: SystemType - List<Instance>   系统类型-系统类型实例列表
        private class OneTypeSystems
        {
            public readonly UnOrderMultiMap<Type, object> Map = new();
            //LCM:系统所对应的 事件队列  参考：InstanceQueueIndex
            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly bool[] QueueFlag = new bool[(int)InstanceQueueIndex.Max];
        }
        
        private class TypeSystems
        {
            //LCM: EntityType - SystemType - List<Instance>        类型-系统类型-系统类型实例列表
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

            //LCM:通过 类型 获取所有 系统类型-系统类型实例列表
            public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new OneTypeSystems();
                this.typeSystemsMap.Add(type, systems);
                return systems;
            }

            //LCM:通过 类型 获取所有 系统类型-系统类型实例列表
            public OneTypeSystems GetOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }

            //LCM:通过 类型-系统类型 获取 系统类型实例列表   EntityType - SystemType - List<Instance>
            public List<object> GetSystems(Type type, Type systemType)
            {
                OneTypeSystems oneTypeSystems = null;
                if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
                {
                    return null;
                }

                if (!oneTypeSystems.Map.TryGetValue(systemType, out List<object> systems))
                {
                    return null;
                }

                return systems;
            }
        }

        private class EventInfo
        {
            public IEvent IEvent { get; }
            
            //LCM:指定由哪种scene触发，如果是SceneType.None 则不限制
            public SceneType SceneType {get; }

            public EventInfo(IEvent iEvent, SceneType sceneType)
            {
                this.IEvent = iEvent;
                this.SceneType = sceneType;
            }
        }
        
        //LCM:所有类型      fullName-Type
        private readonly Dictionary<string, Type> allTypes = new();

        //LCM:   baseAttribute的派生类型 - HashSet<被标记的的类型>
        private readonly UnOrderMultiMapSet<Type, Type> types = new();

        //LCM: 记录所有的事件实例   事件类型 - List< (事件实例引用，sceneType) >
        private readonly Dictionary<Type, List<EventInfo>> allEvents = new();
        
        //LCM: IInvoke.type（参数类型） - Dic< invokeAttribute.type （分发类型）, IInvoke实例 （只有一个）>
        //LCM: 可以理解 参数类型+分发类型 的组合 才是 实例的 key
        private Dictionary<Type, Dictionary<int, object>> allInvokes = new(); 

        //LCM:  EntityType - SystemType - List<Instance>        类型-系统类型-系统类型实例列表
        private TypeSystems typeSystems = new();

        //LCM：由 InstanceQueueIndex 定义事件队列类型（顺序无关）
        private readonly Queue<long>[] queues = new Queue<long>[(int)InstanceQueueIndex.Max];

        public EventSystem()
        {
            for (int i = 0; i < this.queues.Length; i++)
            {
                this.queues[i] = new Queue<long>();
            }
        }

        public void Add(Dictionary<string, Type> addTypes)
        {
            this.allTypes.Clear();
            this.types.Clear();
            
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;
                
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }
            }

            this.typeSystems = new TypeSystems();

            //LCM:被 ObjectSystemAttribute 标记的类将会生成 实例，new()
            foreach (Type type in this.GetTypes(typeof (ObjectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);
                
                if (obj is ISystemType iSystemType)
                {
                    OneTypeSystems oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                    InstanceQueueIndex index = iSystemType.GetInstanceQueueIndex();
                    if (index > InstanceQueueIndex.None && index < InstanceQueueIndex.Max)
                    {
                        oneTypeSystems.QueueFlag[(int)index] = true;
                    }
                }

                //LCM: ObjectSystemAttribute 不一定指向系统，非系统也可以生成实例，感觉可以巧用这一点，（比如不由 Game 管理的 全局单例，只是举例，不要这样用）
                //LCM: 下面的 EventAttribute 与 InvokeAttribute 虽然也会生成实例，但是他们有各自的参数
            }

            //LCM: 被 EventAttribute 标记的类型也会生成 实例，new()
            //LCM: EventAttribute 和 IEvent必须一起使用，最好的方式是继承 AEvent类
            this.allEvents.Clear();
            foreach (Type type in types[typeof (EventAttribute)])
            {
                IEvent obj = Activator.CreateInstance(type) as IEvent;
                if (obj == null)
                {
                    throw new Exception($"type not is AEvent: {type.Name}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
                foreach (object attr in attrs)
                {
                    EventAttribute eventAttribute = attr as EventAttribute;

                    Type eventType = obj.Type;

                    EventInfo eventInfo = new(obj, eventAttribute.SceneType);

                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<EventInfo>());
                    }
                    this.allEvents[eventType].Add(eventInfo);
                }
            }

            //LCM: 被 InvokeAttribute 标记的类型也会生成 实例，new()
            //LCM: InvokeAttribute 和 IInvoke 必须一起使用，最好的方式是继承 AInvokeHandler 类
            this.allInvokes = new Dictionary<Type, Dictionary<int, object>>();
            foreach (Type type in types[typeof (InvokeAttribute)])
            {
                object obj = Activator.CreateInstance(type);
                IInvoke iInvoke = obj as IInvoke;
                if (iInvoke == null)
                {
                    throw new Exception($"type not is callback: {type.Name}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(InvokeAttribute), false);
                foreach (object attr in attrs)
                {
                    if (!this.allInvokes.TryGetValue(iInvoke.Type, out var dict))
                    {
                        dict = new Dictionary<int, object>();
                        this.allInvokes.Add(iInvoke.Type, dict);
                    }
                    
                    InvokeAttribute invokeAttribute = attr as InvokeAttribute;
                    
                    try
                    {
                        dict.Add(invokeAttribute.Type, obj);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"action type duplicate: {iInvoke.Type.Name} {invokeAttribute.Type}", e);
                    }
                    
                }
            }
        }

        //LCM:获取 被指定 baseAttribute 标记的类型
        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }

        //LCM:这里可以发现 注册顺序就是调用顺序
        public void RegisterSystem(Entity component)
        {
            Type type = component.GetType();

            OneTypeSystems oneTypeSystems = this.typeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }
            for (int i = 0; i < oneTypeSystems.QueueFlag.Length; ++i)
            {
                if (!oneTypeSystems.QueueFlag[i])
                {
                    continue;
                }
                this.queues[i].Enqueue(component.InstanceId);
            }
        }

        public void Deserialize(Entity component)
        {
            List<object> iDeserializeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // GetComponentSystem
        public void GetComponent(Entity entity, Entity component)
        {
            List<object> iGetSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSystem));
            if (iGetSystem == null)
            {
                return;
            }

            foreach (IGetComponentSystem getSystem in iGetSystem)
            {
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // AddComponentSystem
        public void AddComponent(Entity entity, Entity component)
        {
            List<object> iAddSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IAddComponentSystem));
            if (iAddSystem == null)
            {
                return;
            }

            foreach (IAddComponentSystem addComponentSystem in iAddSystem)
            {
                if (addComponentSystem == null)
                {
                    continue;
                }

                try
                {
                    addComponentSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake(Entity component)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Load()
        {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.Load];
            int count = queue.Count;
            while (count-- > 0)
            {
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLoadSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILoadSystem));
                if (iLoadSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (ILoadSystem iLoadSystem in iLoadSystems)
                {
                    try
                    {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void Destroy(Entity component)
        {
            List<object> iDestroySystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Update()
        {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.Update];
            int count = queue.Count;
            while (count-- > 0)
            {
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                if (iUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                {
                    try
                    {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void LateUpdate()
        {
            Queue<long> queue = this.queues[(int)InstanceQueueIndex.LateUpdate];
            int count = queue.Count;
            while (count-- > 0)
            {
                long instanceId = queue.Dequeue();
                Entity component = Root.Instance.Get(instanceId);
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLateUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(instanceId);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public async ETTask PublishAsync<T>(Scene scene, T a) where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using ListComponent<ETTask> list = ListComponent<ETTask>.Create();
            
            foreach (EventInfo eventInfo in iEvents)
            {
                if (scene.SceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }
                    
                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }

                list.Add(aEvent.Handle(scene, a));
            }

            try
            {
                await ETTaskHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        //LCM:并没有同步等待，而是以协程的方式并行执行了
        public void Publish<T>(Scene scene, T a) where T : struct
        {
            List<EventInfo> iEvents;
            if (!this.allEvents.TryGetValue(typeof (T), out iEvents))
            {
                return;
            }

            SceneType sceneType = scene.SceneType;
            foreach (EventInfo eventInfo in iEvents)
            {
                if (sceneType != eventInfo.SceneType && eventInfo.SceneType != SceneType.None)
                {
                    continue;
                }

                
                if (!(eventInfo.IEvent is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {eventInfo.IEvent.GetType().Name}");
                    continue;
                }
                
                aEvent.Handle(scene, a).Coroutine();
            }
        }
        
        //LCM: 最主要的区别：Invoke是同步方法，publis是异步方法（无法同步等待）
        // Invoke跟Publish的区别(特别注意)
        // Invoke类似函数，必须有被调用方，否则异常，调用者跟被调用者属于同一模块，比如MoveComponent中的Timer计时器，调用跟被调用的代码均属于移动模块
        // 既然Invoke跟函数一样，那么为什么不使用函数呢? 因为有时候不方便直接调用，比如Config加载，在客户端跟服务端加载方式不一样。比如TimerComponent需要根据Id分发
        // 注意，不要把Invoke当函数使用，这样会造成代码可读性降低，能用函数不要用Invoke
        // publish是事件，抛出去可以没人订阅，调用者跟被调用者属于两个模块，比如任务系统需要知道道具使用的信息，则订阅道具使用事件
        public void Invoke<A>(int type, A args) where A: struct
        {
            if (!this.allInvokes.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(A).Name} {type}");
            }
            
            aInvokeHandler.Handle(args);
        }
        
        public T Invoke<A, T>(int type, A args) where A: struct
        {
            if (!this.allInvokes.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error: {typeof(A).Name}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error: {typeof(A).Name} {type}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A, T>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error, not AInvokeHandler: {typeof(T).Name} {type}");
            }
            
            return aInvokeHandler.Handle(args);
        }
        
        public void Invoke<A>(A args) where A: struct
        {
            Invoke(0, args);
        }
        
        public T Invoke<A, T>(A args) where A: struct
        {
            return Invoke<A, T>(0, args);
        }
    }
}
