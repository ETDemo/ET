using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.ETCodesGenerator
{
    [CreateAssetMenu(menuName = "ET/ETCodesGenerator/Setting", fileName = "Setting")]
    public class Setting: ScriptableObject
    {
        public TextAsset EntityCodeTemplate;
        public TextAsset EntitySystemCodeTemplate_System;
        public TextAsset EntitySystemCodeTemplate_Logic;
    }
}