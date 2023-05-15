using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace ET.GameDemo.Client
{
    public class GlobalHelper : Singleton<GlobalHelper>
    {
        public GameObject GlobalGo { get; private set; }
        public ReferenceCollector GlobalRefcol { get; private set; }
        public Volume GlobalVolume { get; private set; }
        
        public ScreenFadeHelper ScreenFadeHelper { get; private set; }
        public GlobalHelper()
        {
            GlobalGo = GameObject.Find("/Global");
            GlobalRefcol = this.GlobalGo.GetComponent<ReferenceCollector>();

            GlobalVolume = this.GlobalRefcol.Get<GameObject>("GlobalVolume").GetComponent<Volume>();
            ScreenFadeHelper = this.GlobalRefcol.Get<GameObject>("ScreenFadeHelper").GetComponent<ScreenFadeHelper>();
        }
    }
}
