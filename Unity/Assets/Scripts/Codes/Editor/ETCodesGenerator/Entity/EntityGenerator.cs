using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ET.ETCodesGenerator.Entity
{
    //[CreateAssetMenu(menuName = "ET/ETCodesGenerator/EntityGenerator", fileName = "EntityGenerator")]
    public class EntityGenerator: ScriptableObject
    {
        public RootFolderType RootFolderType;

        [InlineButton("SelectSubFolder", ButtonName = "Select")]
        public string SubFolderPath;

        public string SubNameSpace = "YouProjectName";
        public string EntityName;
        public EntityInterfaces Interfaces;
        public EntityType EntityType;
        public bool EntitySingleton;
        public bool OverwriteIfExist;
        public bool DontRefreshAfterGenerate;
        public bool OpenAfterGenerate;

        private Setting _setting;
        private Setting setting => this._setting ??= EditorGUIUtility.Load("ETCodesGenerator/ETCodesGeneratorSetting.asset") as Setting;

        public void Generate()
        {
            var canGenerate = CanGenerate();
            if (!string.IsNullOrEmpty(canGenerate))
                throw new Exception(canGenerate);

            if (this.EntitySingleton)
                Interfaces |= EntityInterfaces.IAwake | EntityInterfaces.IDestroy;

            try
            {
                AssetDatabase.StartAssetEditing();

                this.GenerateEntity();
                this.GenerateSystem();
                this.GenerateSystemLogic();

                if (OpenAfterGenerate)
                {
                    EditorUtility.OpenWithDefaultApp(this.GetEntityFilePath());
                    EditorUtility.OpenWithDefaultApp(this.GetSystemFilePath_Logics());
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                if (!DontRefreshAfterGenerate) AssetDatabase.Refresh();
            }
        }

        public string CanGenerate()
        {
            if (string.IsNullOrEmpty(EntityName))
                return "EntityName 不能为空";

            if (!Regex.IsMatch(this.EntityName, "[_a-zA-Z]+"))
                return "EntityName 命名不规范";

            return string.Empty;
        }

        public string GetEntityFilePath()
        {
            string path = RootFolderType.ToEntityPath();
            return string.IsNullOrEmpty(this.SubFolderPath)? $"{path}/{this.EntityName}/{this.EntityName}.cs"
                    : $"{path}/{this.SubFolderPath}/{this.EntityName}/{this.EntityName}.cs";
        }

        public string GetSystemFilePath_Systems()
        {
            string path = RootFolderType.ToSystemPath();
            return string.IsNullOrEmpty(this.SubFolderPath)? $"{path}/{this.EntityName}/{this.EntityName}.cs"
                    : $"{path}/{this.SubFolderPath}/{this.EntityName}/{this.EntityName}Systems.cs";
        }

        public string GetSystemFilePath_Logics()
        {
            string path = RootFolderType.ToSystemPath();
            return string.IsNullOrEmpty(this.SubFolderPath)? $"{path}/{this.EntityName}/{this.EntityName}.cs"
                    : $"{path}/{this.SubFolderPath}/{this.EntityName}/{this.EntityName}Logics.cs";
        }

        private void GenerateEntity()
        {
            var filePath = this.GetEntityFilePath();

            if (File.Exists(filePath) && !this.OverwriteIfExist)
            {
                Debug.Log("已存在文件，生成失败: " + filePath);
                return;
            }

            //
            var template = setting.EntityCodeTemplate.text;
            template = SetNamespace(template);
            template = SetEntityName(template);
            template = SetEntityType(template);
            template = SetEntityInterfaces(template);
            template = SetEntitySingleton(template);

            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(template);

            Debug.Log("已生成:" + filePath);
        }

        private void GenerateSystem()
        {
            var filePath = this.GetSystemFilePath_Systems();

            if (File.Exists(filePath) && !this.OverwriteIfExist)
            {
                Debug.Log("已存在文件，生成失败: " + filePath);
                return;
            }

            var template = setting.EntitySystemCodeTemplate_System.text;
            template = SetNamespace(template);
            template = SetEntityName(template);
            template = SetSystemInterface(template);
            template = SetEntitySingleton(template);

            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(template);

            Debug.Log("已生成:" + filePath);
        }

        private void GenerateSystemLogic()
        {
            var filePath = this.GetSystemFilePath_Logics();

            if (File.Exists(filePath) && !this.OverwriteIfExist)
            {
                Debug.Log("已存在文件，生成失败: " + filePath);
                return;
            }

            var template = setting.EntitySystemCodeTemplate_Logic.text;
            template = SetNamespace(template);
            template = SetEntityName(template);
            template = SetSystemInterface(template);
            template = SetEntitySingleton(template);

            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using var sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(template);

            Debug.Log("已生成:" + filePath);
        }

        private string SetNamespace(string template)
        {
            var rootNp = this.RootFolderType switch
            {
                RootFolderType.Share => "ET",
                RootFolderType.Server => "ET.Server",
                RootFolderType.Client => "ET.Client",
                RootFolderType.ClientView => "ET.Client",
                _ => throw new ArgumentOutOfRangeException()
            };
            var np = string.IsNullOrEmpty(this.SubNameSpace)? rootNp : $"{rootNp}.{this.SubNameSpace.Trim('.')}";
            return template.Replace("@NAMESPACE@", np);
        }

        private string SetEntityName(string template)
        {
            return template.Replace("@ENTITYNAME@", this.EntityName);
        }

        private string SetEntityInterfaces(string template)
        {
            if (this.Interfaces == EntityInterfaces.None)
                return template.Replace("@INTERFACES@", string.Empty);

            string str = string.Empty;
            foreach (var value in Enum.GetValues(typeof (EntityInterfaces)))
            {
                var v = (EntityInterfaces)value;
                if (v is EntityInterfaces.None or EntityInterfaces.Everything)
                    continue;

                if ((v & this.Interfaces) == 0)
                    continue;

                str += $", {v}";
            }

            return template.Replace("@INTERFACES@", str);
        }

        private string SetEntityType(string template)
        {
            string str = string.Empty;
            if ((this.EntityType & EntityType.Compenonet) != 0)
                str += "[ComponentOf]";
            if ((this.EntityType & EntityType.Child) != 0)
                str += "[ChildOf]";
            return template.Replace("@ENTITYTYPE@", str);
        }

        private string SetEntitySingleton(string template)
        {
            if (!this.EntitySingleton)
            {
                template = ReplaceBetweenStrings(template, "@SINGLETON_BEGIN@", "@SINGLETON_END@", string.Empty);
            }

            template = template.Replace("@SINGLETON_BEGIN@", string.Empty);
            template = template.Replace("@SINGLETON_END@", string.Empty);
            return template;
        }

        private string SetSystemInterface(string template)
        {
            foreach (var value in Enum.GetValues(typeof (EntityInterfaces)))
            {
                var v = (EntityInterfaces)value;
                if (v is EntityInterfaces.None or EntityInterfaces.Everything)
                    continue;

                var begin = $"@{v.ToString().ToUpper()}_BEGIN@";
                var end = $"@{v.ToString().ToUpper()}_END@";
                if ((v & this.Interfaces) == 0)
                {
                    template = ReplaceBetweenStrings(template, begin, end, string.Empty);
                }

                template = template.Replace(begin, string.Empty);
                template = template.Replace(end, string.Empty);
            }

            return template;
        }

        private string ReplaceBetweenStrings(string text, string stringA, string stringB, string replaceWith)
        {
            string pattern = $"(?<={stringA}).*?(?={stringB})";
            return Regex.Replace(text, pattern, replaceWith, RegexOptions.Singleline);
        }

        private void SelectSubFolder()
        {
            var parentFolderPath = this.RootFolderType.ToEntityPath();
            var select = EditorUtility.OpenFolderPanel("Select SubFolder", parentFolderPath, "");
            if (!string.IsNullOrEmpty(select))
            {
                if (select.StartsWith(parentFolderPath))
                {
                    var value = select.Replace(parentFolderPath, "").TrimStart('/');
                    this.SubFolderPath = value;
                }
                else
                {
                    Debug.LogError("只能选择指定目录的子目录");
                }
            }
        }

        private void PingRootFolder()
        {
            var p1 = this.RootFolderType.ToEntityPath();
            var sub1 = "Assets" + p1.Substring(Application.dataPath.Length, p1.Length - Application.dataPath.Length);
            var f1 = AssetDatabase.LoadAssetAtPath<Object>(sub1);
            EditorGUIUtility.PingObject(f1);

            var p2 = this.RootFolderType.ToSystemPath();
            var sub2 = "Assets" + p2.Substring(Application.dataPath.Length, p2.Length - Application.dataPath.Length);
            var f2 = AssetDatabase.LoadAssetAtPath<Object>(sub2);
            EditorGUIUtility.PingObject(f2);

            Debug.Log(sub1 + "  " + sub2);
        }
    }
}