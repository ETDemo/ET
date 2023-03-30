﻿using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            Debug.Log(path);
            //LCM: Assets/Scripts/Core 核心框架代码 （非热更代码）
            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(path, content);
            }

            //LCM: Assets/Scripts/Codes ET内置代码（网络，demo，ui等等） （可热更）
            if (Define.EnableCodes)
            {
                //LCM: Unity 正常（本地模式） 编译 Assets/Scripts/Codes 里的代码 （内置热更程序集）
                //LCM：此时 Unity 正常（本地模式） 编译 Assets/Scripts/Empty  （非内置热更程序集）
                //LCM: 编辑器可访问 非内置 热更程序集
                //LCM: 这样终于可以将 内置代码 与 自定义代码 区分开来
                if (path.EndsWith("Unity.Hotfix.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.Model.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.HotfixView.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.ModelView.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }
            }
            else
            {
                //LCM: Unity 正常（本地模式） 编译Assets/Scripts/Codes里的代码 （内置热更程序集）
                //LCM: 去除 Assets/Scripts/Empty 里的Empty脚本与asmdef，这样Unity就不会编译空文件夹 （非热更程序集）
                if (path.EndsWith("Unity.Hotfix.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\Hotfix\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\Hotfix\\Unity.Hotfix.asmdef\" />", string.Empty);

                    //LCM: link: “（根文件夹）文件匹配模板 （递归匹配）文件全路径的格式化串”
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\Hotfix\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.HotfixView.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\HotfixView\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\HotfixView\\Unity.HotfixView.asmdef\" />", string.Empty);
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\HotfixView\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.Model.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\Model\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\Model\\Unity.Model.asmdef\" />", string.Empty);
                    
                    //LCM:这里要把Mode拆开是因为防止Generate文件夹里出现定义冲突 （最终把 client 和 server的代码都包含了）
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\Model\Server\**\*.cs Server\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Client\**\*.cs Client\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Share\**\*.cs Share\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Generate\ClientServer\**\*.cs Generate\%(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.ModelView.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\ModelView\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\ModelView\\Unity.ModelView.asmdef\" />", string.Empty);
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\ModelView\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }
            }
            return content;
        }

        private static string GenerateCustomProject(string path, string content, params string[] links)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            var newDoc = doc.Clone() as XmlDocument;

            var rootNode = newDoc.GetElementsByTagName("Project")[0];

            //LCM:新的程序集引用指定文件夹里的所有脚本
            XmlElement itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
            foreach (var s in links)
            {
                string[] ss = s.Split(' ');
                string p = ss[0];
                string linkStr = ss[1];
                XmlElement compile = newDoc.CreateElement("Compile", newDoc.DocumentElement.NamespaceURI);
                XmlElement link = newDoc.CreateElement("Link", newDoc.DocumentElement.NamespaceURI);
                link.InnerText = linkStr;
                compile.AppendChild(link);
                compile.SetAttribute("Include", p);
                itemGroup.AppendChild(compile);
            }

            //LCM:设置（代码）分析器（用于代码规范检查）
            var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
            projectReference.SetAttribute("Include", @"..\Share\Analyzer\Share.Analyzer.csproj");
            projectReference.SetAttribute("OutputItemType", @"Analyzer");
            projectReference.SetAttribute("ReferenceOutputAssembly", @"false");

            var project = newDoc.CreateElement("Project", newDoc.DocumentElement.NamespaceURI);
            project.InnerText = @"{d1f2986b-b296-4a2d-8f12-be9f470014c3}";
            projectReference.AppendChild(project);

            var name = newDoc.CreateElement("Name", newDoc.DocumentElement.NamespaceURI);
            name.InnerText = "Analyzer";
            projectReference.AppendChild(project);

            itemGroup.AppendChild(projectReference);

            rootNode.AppendChild(itemGroup);

            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    tx.Formatting = Formatting.Indented;
                    newDoc.WriteTo(tx);
                    tx.Flush();
                    return sw.GetStringBuilder().ToString();
                }
            }
        }
    }
}