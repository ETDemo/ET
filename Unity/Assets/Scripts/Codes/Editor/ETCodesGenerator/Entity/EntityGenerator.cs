using System;
using UnityEngine;

namespace ET.ETCodesGenerator.Entity
{
    [CreateAssetMenu(menuName = "ET/ETCodesGenerator/EntityGenerator", fileName = "EntityGenerator")]
    public class EntityGenerator: ScriptableObject
    {
        public RootFolderType RootFolderType;
        public string SubFolder;
        public string NameSpace = "ET.YouProjectName";
        public string EntityName;
        public EntityInterfaces Interfaces;
        public bool EntitySingleton;

        public void Generate()
        {
            var canGenerate = CanGenerate();
            if (!string.IsNullOrEmpty(canGenerate))
                throw new Exception(canGenerate);

            if (this.EntitySingleton)
                Interfaces |= EntityInterfaces.IAwake | EntityInterfaces.IDestroy;

            GenerateEntity();
            GenerateSystem();
        }

        public string CanGenerate()
        {
            if (string.IsNullOrEmpty(NameSpace))
                return "NameSpace 不能为空";
            if (string.IsNullOrEmpty(EntityName))
                return "EntityName 不能为空";

            return string.Empty;
        }

        public string GetEntityFilePath()
        {
            string path = RootFolderType switch
            {
                RootFolderType.Share => PathHelper.ShareModelFolder,
                RootFolderType.Server => PathHelper.ServerModelFolder,
                RootFolderType.Client => PathHelper.ClientModelFolder,
                RootFolderType.ClientView => PathHelper.ClientModelViewFolder,
                _ => throw new ArgumentOutOfRangeException()
            };
            return string.IsNullOrEmpty(this.SubFolder)? $"{path}/{this.EntityName}" : $"{path}/{this.SubFolder}/{this.EntityName}";
        }

        public string GetSystemFilePath()
        {
            string path = RootFolderType switch
            {
                RootFolderType.Share => PathHelper.ShareHotfixFolder,
                RootFolderType.Server => PathHelper.ServerHotfixFolder,
                RootFolderType.Client => PathHelper.ClientHotfixFolder,
                RootFolderType.ClientView => PathHelper.ClientHotfixViewFolder,
                _ => throw new ArgumentOutOfRangeException()
            };
            return string.IsNullOrEmpty(this.SubFolder)? $"{path}/{this.EntityName}" : $"{path}/{this.SubFolder}/{this.EntityName}";
        }

        private void GenerateEntity()
        {
        }

        private void GenerateSystem()
        {
        }
    }
}