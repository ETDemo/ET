using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

namespace ET
{
    /* LCM：
     * Codes里的程序集平台都是Editor （打包时不会包含）
     * 在编辑器里： 所以 当 ENABLE_CODES 开启的时候，Empty里的程序集没有引用Codes里的脚本，所以一直是以编辑器模式运行。
     * 在编辑器里： 当 ENABLE_CODES 未开启的时候，Empty里的程序集会引用Codes里的脚本。所以Codes里的代码千万不要引用Editor的命名空间。
     * 要用Empty来给Codes套壳，目的是为了既可以编辑器模式访问热更代码（ENABLE_CODES启用不启用都可以访问）， 又可以打包的时候不包含热更代码。
     * 热更代码编译 的时候也是直接从Codes里引用脚本的，所以Empty里面不要有脚本。
     * 因为Codes的平台是Editor，所以即使是本地模式打包，也要采取读取热更代码的方式 。
     * 可是这样导致一个问题，就是Codes引用的额外程序集，Empty必须要同步才行。
     */
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        //LCM:这个只是在Unity编辑器资源更新时调用，打包 的时候不会调用
        public static string OnGeneratedCSProject(string path, string content)
        {
            //LCM: Assets/Scripts/Core 核心框架代码 （非热更代码）
            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(path, content);
            }

            //LCM: Assets/Scripts/Codes  （可热更）
            if (Define.EnableCodes)
            {
                //LCM：此时 Unity 正常（本地模式） 编译 
                //LCM: Unity编辑器运行的是Codes程序集代码
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
                //LCM: 而Empty里的程序集 会引用 .codes程序集里的代码，也就是包含了 .codes （这样的，那么Empty和Codes程序集对其它的额外程序集的引用必须要同步啊！可是作者没处理啊？）
                //LCM: Unity编辑器运行的是Empty程序集代码
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