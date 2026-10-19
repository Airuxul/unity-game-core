namespace Air.UnityGameCore.Runtime.Event
{
    /// <summary>
    /// 框架事件。占用 1..999。游戏自己的事件从 1000 起，只追加，不改已有值。
    /// </summary>
    public enum RuntimeEventId : uint
    {
        EntityCreated = 1,
        EntityDestroyed = 2,
        SceneLoadStarted = 3,
        SceneLoadCompleted = 4,
    }
}
