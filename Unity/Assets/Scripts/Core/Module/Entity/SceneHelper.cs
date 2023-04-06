namespace ET
{
    public static class SceneHelper
    {
        //LCM: 后去Scene所属的 Zone 的ID
        public static int DomainZone(this Entity entity)
        {
            return ((Scene) entity.Domain)?.Zone ?? 0;
        }

        public static Scene DomainScene(this Entity entity)
        {
            return (Scene) entity.Domain;
        }
    }
}