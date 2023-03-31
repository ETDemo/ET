using System;

namespace ET
{
    //LCM:指定父级 [ChildOf(Enity type)]
    //LCM:不指定父级 [ChildOf]
    [AttributeUsage(AttributeTargets.Class)]
    public class ChildOfAttribute : Attribute
    {
        public Type type;

        public ChildOfAttribute(Type type = null)
        {
            this.type = type;
        }
    }
}