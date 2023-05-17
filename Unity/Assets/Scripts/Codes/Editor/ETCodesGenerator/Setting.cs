using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.ETCodesGenerator
{
    [CreateAssetMenu(menuName = "ET/ETCodesGenerator/Setting", fileName = "Setting")]
    public class Setting: ScriptableObject
    {
        public List<string> SubFolders = new List<string>();
    }
}