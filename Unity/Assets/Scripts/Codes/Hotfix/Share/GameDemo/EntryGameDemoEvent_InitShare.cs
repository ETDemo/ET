using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.GameDemo
{
    [Event(SceneType.Process)]
    public class EntryGameDemoEvent_InitShare : AEvent<EventType.EntryGameDemoEvent_InitShare>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryGameDemoEvent_InitShare a)
        {
            Game.AddSingleton<NetServices>();
            
            await Game.AddSingleton<ConfigComponent>().LoadAsync();
            
            Game.AddSingleton<RsaPublic>();
            
            Root.Instance.Scene.AddComponent<NetThreadComponent>();
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<AIDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            
        }
    }
}
