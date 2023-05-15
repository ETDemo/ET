namespace ET
{
    namespace EventType
    {
        public struct EntryEvent1
        {
        }   
        
        public struct EntryEvent2
        {
        } 
        
        public struct EntryEvent3
        {
        }
    }
    
    public static class Entry
    {
        public static void Init()
        {
            
        }
        
        public static void Start()
        {
            //StartAsync().Coroutine();
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

        private static async ETTask StartAsync()
        {
            WinPeriod.Init();
            
            MongoHelper.Init();
            ProtobufHelper.Init();
            
            Game.AddSingleton<NetServices>();
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}