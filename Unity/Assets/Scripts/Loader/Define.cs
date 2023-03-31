namespace ET
{
	public static class Define
	{
		public const string BuildOutputDir = "./Temp/Bin/Debug";

		//LCM:举例: 加载assetbundle包时，是否采用异步模式
#if UNITY_EDITOR && !ASYNC
		public static bool IsAsync = false;
#else
        public static bool IsAsync = true;
#endif
		
#if UNITY_EDITOR
		public static bool IsEditor = true;
#else
        public static bool IsEditor = false;
#endif
		//LCM: Codes程序集与Empty程序集 地位相等（最高层）
		//LCM: Codes程序集.asmdef添加了 Define Constraints ,所以只有 ENABLE_CODES 开启的时候，Codes程序集才会被编译  (编辑器脚本也有定义约束)
		//LCM: OnGenerateCSProjectProcessor.cs 对生成的CS程序集做了处理 （实测: build的时候不会触发，只有编辑器模里，刷新工程时，才触发）
		//LCM：ENABLE_CODES 开启时，编辑器运行的是Codes程序集 （Empty里的程序集未引用Codes里的脚本）
		//LCM：ENABLE_CODES 关闭时，编辑器运行的是Empty（引用了Codes里的所有脚本）里的程序集。（Codes程序集不会被编译）（Ps:这里要手动同步Empty程序集和Codes程序集对其它程序集的引用）
		//LCM: 在打包的时候，由于Codes程序集的平台是Editor，所以Codes程序集不会被编译，所以如果要打本地包，必须去关闭 ENABLE_CODES，这样Empty就会引用Codes里的脚本了。
		//LCM: 打热更代码资源时，是直接引用的Codes程序集里的脚本组成新的程序集（与Empty无关），所以 ENABLE_CODES 开启和关闭无所谓 (反正打包时不会编译平台为Editor的 Codes程序集)
		//LCM: ENABLE_CODES的目的仅仅是给编辑器使用，热更代码里不要使用 该宏定义
		//LCM: 个人觉得开发时，始终开启 ENABLE_CODES 比较好（本地模式，打包时关闭一下），有编辑器视图，还少了手动操作，无非是多编译了Editor程序集罢了。
#if ENABLE_CODES
		public static bool EnableCodes = true;
#else
        public static bool EnableCodes = false;
#endif
		
		//LCM： ENABLE_VIEW 的目的在于编辑器可以显示Entity的层级
		//LCM: 开启后 Entity 在 register 的时候会实例化一个Gameobbject,并附带 ComponentView 脚本
		//LCM: ENABLE_CODES 不会影响 ENABLE_VIEW 显示视图
		//LCM: 热更代码里不要使用 该宏定义
#if ENABLE_VIEW
		public static bool EnableView = true;
#else
		public static bool EnableView = false;
#endif
		
#if ENABLE_IL2CPP
		public static bool EnableIL2CPP = true;
#else
		public static bool EnableIL2CPP = false;
#endif
		
		public static UnityEngine.Object LoadAssetAtPath(string s)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
#else
			return null;
#endif
		}
		
		public static string[] GetAssetPathsFromAssetBundle(string assetBundleName)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
#else
			return new string[0];
#endif
		}
		
		public static string[] GetAssetBundleDependencies(string assetBundleName, bool v)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.GetAssetBundleDependencies(assetBundleName, v);
#else
			return new string[0];
#endif
		}
	}
}