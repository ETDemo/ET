using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ET
{
	public class CodeLoader: Singleton<CodeLoader>
	{
		private Assembly model;

		public void Start()
		{
			//LCM:Play模式运行时，只有刚开始的时候会判断 Define.EnableCodes ，Play模式运行之后对 ENABLE_CODES 的修改无意义
			if (Define.EnableCodes)
			{
				GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					//LCM:其实和ClientSever无关，真正的约束是 只能是编辑器模式，因为Codes代码平台是editor，不会被打包，所以打包后只能采取热更代码的方式
					throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
				}
				
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
				EventSystem.Instance.Add(types);
				foreach (Assembly ass in assemblies)
				{
					string name = ass.GetName().Name;
					if (name == "Unity.Model.Codes")
					{
						this.model = ass;
					}
				}
			}
			else
			{
				byte[] assBytes;
				byte[] pdbBytes;
				if (!Define.IsEditor)
				{
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;
					pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;

					if (Define.EnableIL2CPP)
					{
						HybridCLRHelper.Load();
					}
				}
				else
				{
					assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
				}
			
				this.model = Assembly.Load(assBytes, pdbBytes);
				this.LoadHotfix();
			}
			
			IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
			start.Run();
		}

		//LCM:通过以下代码发现，如果是本地模式，使用的是原生c#的热重载，但是并没有更新 EventSystem 里的类型记录
		//LCM：甚至，如果是本地模式，调用这个方法还可能出错，因为读取了不希望读取的 codes 资源文件
		// 热重载调用该方法
		public void LoadHotfix()
		{
			byte[] assBytes;
			byte[] pdbBytes;
			if (!Define.IsEditor)
			{
				Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
				assBytes = ((TextAsset)dictionary["Hotfix.dll"]).bytes;
				pdbBytes = ((TextAsset)dictionary["Hotfix.pdb"]).bytes;
			}
			else
			{
				// 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
				string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
				if (logicFiles.Length != 1)
				{
					throw new Exception("Logic dll count != 1");
				}
				string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
				assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
				pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));
			}

			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
			
			Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.model, hotfixAssembly);
			
			EventSystem.Instance.Add(types);
		}
	}
}