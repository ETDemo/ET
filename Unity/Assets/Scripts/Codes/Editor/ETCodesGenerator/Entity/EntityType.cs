using System;
namespace ET.ETCodesGenerator.Entity
{
    [Flags]
    public enum EntityType: byte
    {
        None = 0,
        Everything = byte.MaxValue, 
        
        Compenonet = 1,
        Child = 1 << 1,
    }
}
