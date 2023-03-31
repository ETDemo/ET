using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EventAttribute: BaseAttribute
	{
		//LCM:指定可以发布事件的Scene类型，如果为SceneType.none则不限制
		public SceneType SceneType { get; }

		public EventAttribute(SceneType sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}