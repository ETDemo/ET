using System.Collections;
using System.Collections.Generic;
using ET.Client;
using UnityEngine;

namespace ET.GameDemo
{
    [Event(SceneType.Process)]
    public class EntryGameDemoEvent : AEvent<EventType.EntryGameDemoEvent_InitClient>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryGameDemoEvent_InitClient a)
        {
            await ETTask.CompletedTask;
        }
    }
}
