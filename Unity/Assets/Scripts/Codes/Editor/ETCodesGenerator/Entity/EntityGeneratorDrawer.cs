using System;
using UnityEditor;
using UnityEngine;

namespace ET.ETCodesGenerator.Entity
{
    internal class EntityGeneratorWindow: EditorWindow
    {
        [MenuItem("ET/ETCodesGenerator/生成Entity代码")]
        public static void OpenWindow()
        {
            var w = EditorWindow.GetWindow<EntityGeneratorWindow>();
            w.titleContent = new GUIContent("生成Entity代码");
            w.Show();
        }

        private EntityGenerator _so;
        private EntityGenerator so => this._so ??= EditorGUIUtility.Load("ETCodesGenerator/EntityGenerator.asset") as EntityGenerator;

        private Editor _drawer;
        private Editor drawer => this._drawer ??= Editor.CreateEditor(this.so);

        private Vector2 scrollPos = Vector2.zero;

        private void OnGUI()
        {
            if (this.so == null)
            {
                GUILayout.Label("未找到 EntityGenerator 资源文件");
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, false, false))
            {
                drawer.OnInspectorGUI();
                scrollPos = scroll.scrollPosition;
            }
        }

        private void OnDestroy()
        {
            if (this.drawer != null)
                DestroyImmediate(this.drawer);
        }
    }

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

            if (eg.EntitySingleton && ((eg.Interfaces & EntityInterfaces.IAwake) == 0 || (eg.Interfaces & EntityInterfaces.IDestroy) == 0))
            {
                eg.Interfaces |= EntityInterfaces.IAwake | EntityInterfaces.IDestroy;
                EditorUtility.SetDirty(eg);
            }

            DrawMsg(this.msg, Color.yellow);

            GUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
            DrawMsg($"Entity脚本路径:\n{eg.GetEntityFilePath()}\n\nSystem脚本路径:\n{eg.GetSystemFilePath_Systems()}\n\n{eg.GetSystemFilePath_Logics()}",
                Color.green);

            var oldColor = GUI.color;
            GUI.color = Color.green;
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
                    throw;
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