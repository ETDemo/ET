using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.MyProject
{
    [ChildOf()]
    [ComponentOf]
    public partial class TestEntity: Entity,
            ILoad,
            IAwake,
            IUpdate, 
            ILateUpdate, 
            IDestroy,
            IGetComponent,
            IAddComponent,
            IDeserialize,
            ISerializeToEntity,
            ITransfer
    {
        [StaticField]
        public static TestEntity _instance;

        public static TestEntity Instance => _instance;
    }
}
