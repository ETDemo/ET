using UnityEngine;

namespace ET.ETCodesGenerator
{
    public static class PathHelper
    {
        public static readonly string CodesFolder = CombinePath(Application.dataPath, "Codes");

        public static readonly string ModelFolder = CombinePath(CodesFolder, "Model");

        public static readonly string ModelViewFolder = CombinePath(CodesFolder, "ModelView");

        public static readonly string HotfixFolder = CombinePath(CodesFolder, "HotfixVie");

        public static readonly string HotfixViewFolder = CombinePath(CodesFolder, "HotfixView");

        //----------

        public static readonly string ShareModelFolder = CombinePath(ModelFolder, "Share");

        public static readonly string ShareHotfixFolder = CombinePath(HotfixFolder, "Share");

        public static readonly string ServerModelFolder = CombinePath(ModelFolder, "Server");

        public static readonly string ServerHotfixFolder = CombinePath(HotfixFolder, "Server");

        public static readonly string ClientModelFolder = CombinePath(ModelFolder, "Client");

        public static readonly string ClientHotfixFolder = CombinePath(HotfixFolder, "Client");

        public static readonly string ClientModelViewFolder = CombinePath(ModelViewFolder, "Client");

        public static readonly string ClientHotfixViewFolder = CombinePath(HotfixViewFolder, "Client");

        private static string CombinePath(string path1, string path2)
        {
            //return Path.Combine(path1, path2);
            if (string.IsNullOrEmpty(path2)) return path1;
            if (string.IsNullOrEmpty(path1)) return path2;
            return $"{path1}/{path2}";
        }
    }
}