namespace ET.GameDemo
{
    public static class GameDemoEntry
    {
        public static void Start()
        {
            StartGameAsync().Coroutine();
        }

        private static async ETTask StartGameAsync()
        {
            WinPeriod.Init();
            MongoHelper.Init();
            ProtobufHelper.Init();

            Game.AddSingleton<NetServices>();
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new GameDemo.EventType.EntryGameDemoEvent_InitShare());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new GameDemo.EventType.EntryGameDemoEvent_InitServer());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new GameDemo.EventType.EntryGameDemoEvent_InitClient());
        }
    }
}