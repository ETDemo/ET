namespace ET
{
    //LCM: 注意区分 system.object、 unityEngine.Object 、ET.Object
    public abstract class Object
    {
        public override string ToString()
        {
            return JsonHelper.ToJson(this);
        }
    }
}