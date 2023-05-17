using System;
using UnityEditor;
using UnityEngine;

namespace ET.ETCodesGenerator.Entity
{
    [CustomEditor(typeof (EntityGenerator))]
    internal class EntityGeneratorDrawer: Editor
    {
        public string errorMsg = string.Empty;
        public string msg = string.Empty;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //
            var eg = this.target as EntityGenerator;

            // if (eg.EntitySingleton)
            // {
            //     eg.Interfaces |= EntityInterfaces.IAwake | EntityInterfaces.IDestroy;
            //     EditorUtility.SetDirty(eg);
            // }

            DrawMsg(this.msg, Color.yellow);

            GUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
            var oldColor = GUI.color;
            GUI.color = Color.green;
            GUILayout.Label($"Entity脚本路径： {eg.GetEntityFilePath()}");
            GUILayout.Label($"System脚本路径： {eg.GetSystemFilePath()}");

            var oldEnable = GUI.enabled;
            GUI.enabled = CanGenerate();
            GUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
            if (GUILayout.Button("Generate", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                try
                {
                    eg.Generate();
                    errorMsg = string.Empty;
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                }
            }

            GUI.enabled = oldEnable;
            GUI.color = oldColor;

            DrawMsg(this.errorMsg, Color.red);
        }

        private void DrawMsg(string message, Color color)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var oldColor = GUI.color;
                GUI.color = color;
                GUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
                GUILayout.Box($"\n{message}\n", GUILayout.Width(EditorGUIUtility.currentViewWidth));
                GUI.color = oldColor;
            }
        }

        private bool CanGenerate()
        {
            var eg = this.target as EntityGenerator;
            var ret = eg.CanGenerate();
            msg = ret;
            return string.IsNullOrEmpty(this.msg);
        }
    }
}