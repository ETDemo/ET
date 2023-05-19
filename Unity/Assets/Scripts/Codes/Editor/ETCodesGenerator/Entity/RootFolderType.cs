using System;

namespace ET.ETCodesGenerator.Entity
{
    public enum RootFolderType
    {
        Share,
        Server,
        Client,
        ClientView
    }

    public static class RootFolderTypeHelper
    {
        public static string ToEntityPath(this RootFolderType folderType) => folderType switch
        {
            RootFolderType.Share => PathHelper.ShareModelFolder,
            RootFolderType.Server => PathHelper.ServerModelFolder,
            RootFolderType.Client => PathHelper.ClientModelFolder,
            RootFolderType.ClientView => PathHelper.ClientModelViewFolder,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static string ToSystemPath(this RootFolderType folderType) => folderType switch
        {
            RootFolderType.Share => PathHelper.ShareHotfixFolder,
            RootFolderType.Server => PathHelper.ServerHotfixFolder,
            RootFolderType.Client => PathHelper.ClientHotfixFolder,
            RootFolderType.ClientView => PathHelper.ClientHotfixViewFolder,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}