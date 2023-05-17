using System;
using System.IO;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ET.ETCodesGenerator
{
    public class CodesSubFolderAttribute: Attribute
    {
        public string ParentFolder;
        public CodesSubFolderAttribute(string parentFolder) => ParentFolder = parentFolder;
    }

    public class CodesSubFolderAttributeDrawer: OdinAttributeDrawer<CodesSubFolderAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                CallNextDrawer(label);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    string parentFolderPath = PathHelper.CodesFolder;
                    if (!string.IsNullOrEmpty(Attribute.ParentFolder))
                    {
                        if (Attribute.ParentFolder.StartsWith('$'))
                        {
                            var obj = this.Property.Parent.ValueEntry.WeakSmartValue;
                            var pName = Attribute.ParentFolder.TrimStart('$');
                            var p = obj.GetType().GetProperty(pName,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.NonPublic);
                            parentFolderPath = p.GetValue(obj) as string;
                        }
                        else
                        {
                            parentFolderPath = Attribute.ParentFolder;
                        }
                    }

                    if (parentFolderPath.StartsWith(PathHelper.CodesFolder))
                    {
                        var select = EditorUtility.OpenFolderPanel("Select SubFolder", parentFolderPath, "");
                        if (!string.IsNullOrEmpty(select) && select.StartsWith(parentFolderPath))
                        {
                            var value = select.Replace(parentFolderPath, "").TrimStart('/');
                            this.Property.ValueEntry.WeakSmartValue = value;
                            this.Property.ValueEntry.ApplyChanges();
                        }
                    }
                    else
                    {
                        Debug.LogError("非ET工程中的路径");
                    }
                }
            }
        }
    }
}