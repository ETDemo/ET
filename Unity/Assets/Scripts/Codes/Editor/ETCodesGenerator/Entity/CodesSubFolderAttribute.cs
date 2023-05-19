using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET.ETCodesGenerator
{
    public class CodesSubFolderAttribute: PropertyAttribute
    {
        public string ParentFolder;
        public CodesSubFolderAttribute(string parentFolder) => ParentFolder = parentFolder;
    }

    [CustomPropertyDrawer(typeof (CodesSubFolderAttribute))]
    public class CodesSubFolderAttributeDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as CodesSubFolderAttribute;

            var btnWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - EditorGUIUtility.fieldWidth;
            btnWidth = Mathf.Clamp(btnWidth, 0, EditorGUIUtility.fieldWidth);
            var rect = position;
            rect.width -= btnWidth;
            EditorGUI.PropertyField(rect, property, label);
            rect = position;
            rect.width = btnWidth;
            rect.center += new Vector2(position.width - btnWidth, 0);
            if (GUI.Button(rect, "Select"))
            {
                string parentFolderPath = PathHelper.CodesFolder;
                if (!string.IsNullOrEmpty(attrib.ParentFolder))
                {
                    if (attrib.ParentFolder.StartsWith('$'))
                    {
                        var obj = property.serializedObject.targetObject;
                        var pName = attrib.ParentFolder.TrimStart('$');
                        var p = obj.GetType().GetProperty(pName,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.NonPublic);
                        parentFolderPath = p.GetValue(obj) as string;
                    }
                    else
                    {
                        parentFolderPath = attrib.ParentFolder;
                    }
                }

                if (parentFolderPath.StartsWith(PathHelper.CodesFolder))
                {
                    var select = EditorUtility.OpenFolderPanel("Select SubFolder", parentFolderPath, "");
                    if (!string.IsNullOrEmpty(select) && select.StartsWith(parentFolderPath))
                    {
                        var value = select.Replace(parentFolderPath, "").TrimStart('/');
                        property.stringValue = value;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    Debug.LogError("非ET工程中的路径");
                }
            }
        }
    }

    // public class CodesSubFolderAttributeDrawer: OdinAttributeDrawer<CodesSubFolderAttribute>
    // {
    //     protected override void DrawPropertyLayout(GUIContent label)
    //     {
    //         using (var hor = new EditorGUILayout.HorizontalScope())
    //         {
    //             CallNextDrawer(label);
    //             if (GUILayout.Button("Select", GUILayout.Width(60)))
    //             {
    //                 string parentFolderPath = PathHelper.CodesFolder;
    //                 if (!string.IsNullOrEmpty(Attribute.ParentFolder))
    //                 {
    //                     if (Attribute.ParentFolder.StartsWith('$'))
    //                     {
    //                         var obj = this.Property.Parent.ValueEntry.WeakSmartValue;
    //                         var pName = Attribute.ParentFolder.TrimStart('$');
    //                         var p = obj.GetType().GetProperty(pName,
    //                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.NonPublic);
    //                         parentFolderPath = p.GetValue(obj) as string;
    //                     }
    //                     else
    //                     {
    //                         parentFolderPath = Attribute.ParentFolder;
    //                     }
    //                 }
    //
    //                 if (parentFolderPath.StartsWith(PathHelper.CodesFolder))
    //                 {
    //                     var select = EditorUtility.OpenFolderPanel("Select SubFolder", parentFolderPath, "");
    //                     if (!string.IsNullOrEmpty(select) && select.StartsWith(parentFolderPath))
    //                     {
    //                         var value = select.Replace(parentFolderPath, "").TrimStart('/');
    //                         this.Property.ValueEntry.WeakSmartValue = value;
    //                         this.Property.ValueEntry.ApplyChanges();
    //                     }
    //                 }
    //                 else
    //                 {
    //                     Debug.LogError("非ET工程中的路径");
    //                 }
    //             }
    //         }
    //     }
    //}
}