using System;

namespace ET.MyProject
{
    [FriendOf(typeof (TestEntity))]
    public static partial class TestEntitySystem
    {
        [ObjectSystem]
        internal class TestEntityAwakeSystem: AwakeSystem<TestEntity>
        {
            protected override void Awake(TestEntity self)
            {
                if (TestEntity.Instance != null)
                    throw new Exception("已存在单例:" + typeof (TestEntity));
                TestEntity._instance = self;

                self.OnAwake();
            }
        }

        [ObjectSystem]
        internal class TestEntityDestroySystem: DestroySystem<TestEntity>
        {
            protected override void Destroy(TestEntity self)
            {
                self.OnDestroy();
                
                if (TestEntity.Instance == self)
                    TestEntity._instance = null;
            }
        }

        private static void OnAwake(this TestEntity self)
        {
        }

        private static void OnDestroy(this TestEntity self)
        {
        }
    }
}