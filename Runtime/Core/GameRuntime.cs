using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Resource;

namespace Air.UnityGameCore.Runtime
{
    /// <summary>
    /// 默认游戏运行时：事件总线与资源加载。由游戏入口持有实例，无全局单例。
    /// </summary>
    public sealed class GameRuntime : IGameRuntime
    {
        public EventBus Events { get; }
        public IResManager Resources { get; }

        public GameRuntime(IResManager resources)
        {
            Resources = resources ?? throw new System.ArgumentNullException(nameof(resources));
            Events = new EventBus();
        }

        public static GameRuntime CreateDefault() => new(new UnityResManager());
    }
}
