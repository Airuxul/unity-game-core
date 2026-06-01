using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Resource;

namespace Air.UnityGameCore.Runtime
{
    public interface IGameRuntime
    {
        EventBus Events { get; }
        IResManager Resources { get; }
    }
}
