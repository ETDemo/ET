using System;

namespace ET.ETCodesGenerator.Entity
{
    [Flags]
    public enum EntityInterfaces: uint
    {
        None = 0,
        Everything = uint.MaxValue, 
        
        IAwake = 1 << 1,
        IUpdate = 1 << 2,
        ILateUpdate = 1 << 3,
        IDestroy = 1 << 4,
        ILoad = 1 << 5,
        IGetComponent = 1 << 6,
        IAddComponent = 1 << 7,
        IDeserialize = 1 << 8,
        ISerializeToEntity = 1 << 9,
        ITransfer = 1 << 10,
    }
}