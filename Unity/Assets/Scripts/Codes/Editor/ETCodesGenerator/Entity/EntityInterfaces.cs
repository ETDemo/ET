using System;

namespace ET.ETCodesGenerator.Entity
{
    [Flags]
    public enum EntityInterfaces: uint
    {
        None = 0,
        Everything = 0xFFFFFFFF,

        IAwake = 1 << 1,
        IDestroy = 1 << 2,
        ILoad = 1 << 3,
        IUpdate = 1 << 4,
        ILateUpdate = 1 << 5,
        IGetComponent = 1 << 6,
        IAddComponent = 1 << 7,
        IDeserialize = 1 << 8,
        ISerializeToEntity = 1 << 9,
        ITransfer = 1 << 10,
    }
}