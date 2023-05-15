using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ET.Client;
using UnityEngine;

namespace ET.GameDemo.Client
{
    [Event(SceneType.Process)]
    public class EntryGameDemoEvent : AEvent<EventType.EntryGameDemoEvent_InitClient>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryGameDemoEvent_InitClient a)
        {
            await ETTask.CompletedTask;
            
            Game.AddSingleton<GlobalHelper>();
            
            //等待一下，让该初始化完毕的都初始化完
            await TimerComponent.Instance.WaitAsync(1000);
            
            await GlobalHelper.Instance.ScreenFadeHelper.FadeToWhite();
        }
    }
}
