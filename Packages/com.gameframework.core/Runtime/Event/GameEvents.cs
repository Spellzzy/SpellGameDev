namespace GameFramework.Event
{
    /// <summary>
    /// 框架内置事件名称常量定义。
    /// 项目业务事件建议在 GameContent 中另建常量类。
    /// </summary>
    public static class GameEvents
    {
        // ── 应用生命周期 ──
        public const string APP_INIT_COMPLETE = "App.InitComplete";
        public const string APP_PAUSE = "App.Pause";
        public const string APP_RESUME = "App.Resume";
        public const string APP_QUIT = "App.Quit";

        // ── 场景 ──
        public const string SCENE_LOAD_START = "Scene.LoadStart";
        public const string SCENE_LOAD_PROGRESS = "Scene.LoadProgress";
        public const string SCENE_LOAD_COMPLETE = "Scene.LoadComplete";
        public const string SCENE_UNLOAD_COMPLETE = "Scene.UnloadComplete";

        // ── 游戏状态 ──
        public const string GAME_STATE_CHANGED = "GameState.Changed";

        // ── UI ──
        public const string UI_PANEL_OPENED = "UI.PanelOpened";
        public const string UI_PANEL_CLOSED = "UI.PanelClosed";

        // ── 资源 ──
        public const string RESOURCE_LOAD_COMPLETE = "Resource.LoadComplete";
        public const string RESOURCE_LOAD_FAILED = "Resource.LoadFailed";

        // ── 音频 ──
        public const string AUDIO_BGM_CHANGED = "Audio.BgmChanged";
        public const string AUDIO_VOLUME_CHANGED = "Audio.VolumeChanged";

        // ── 存档 ──
        public const string SAVE_COMPLETE = "Save.Complete";
        public const string LOAD_COMPLETE = "Save.LoadComplete";
    }
}
