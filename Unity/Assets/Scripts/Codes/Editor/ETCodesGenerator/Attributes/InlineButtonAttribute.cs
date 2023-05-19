using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET.ETCodesGenerator
{
    public class InlineButtonAttribute: PropertyAttribute
    {
        public string FuncName;
        public float Width = 50;
        public string ButtonName = "Button";
        public InlineButtonAttribute(string funcName) => this.FuncName = funcName;
    }

    [CustomPropertyDrawer(typeof (InlineButtonAttribute))]
    public class InlineButtonAttributeDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as InlineButtonAttribute;

            var btnWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - EditorGUIUtility.fieldWidth;
            btnWidth = Mathf.Clamp(btnWidth, 0, attrib.Width);
            var rect = position;
            rect.width -= btnWidth;
            EditorGUI.PropertyField(rect, property, label);
            rect = position;
            rect.width = btnWidth;
            rect.center += new Vector2(position.width - btnWidth, 0);

            if (GUI.Button(rect, attrib.ButtonName))
            {
                if (!string.IsNullOrEmpty(attrib.FuncName))
                {
                    var obj = property.serializedObject.targetObject;
                    var fName = attrib.FuncName;
                    var m = obj.GetType().GetMethod(fName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                    m.Invoke(obj, null);
                }
            }
        }
    }
}