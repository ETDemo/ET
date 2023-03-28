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
		
		//LCM： ENABLE_CODES 的目的在于编辑器可以访问热更逻辑代码
#if ENABLE_CODES
		public static bool EnableCodes = true;
#else
        public static bool EnableCodes = false;
#endif
		
		//LCM： ENABLE_VIEW 的目的在于编辑器可以显示Entity的层级
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