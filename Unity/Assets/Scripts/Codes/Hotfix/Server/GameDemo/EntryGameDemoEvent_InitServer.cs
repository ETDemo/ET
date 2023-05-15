using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.GameDemo.Server
{
    [Event(SceneType.Process)]
    public class EntryGameDemoEvent_InitServer : AEvent<EventType. EntryGameDemoEvent_InitServer>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryGameDemoEvent_InitServer a)
        {
            Game.AddSingleton<RsaPrivate>();
            await ETTask.CompletedTask;
        }
    }
}
