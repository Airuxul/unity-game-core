using UnityEngine;

namespace Air.UnityGameCore.Runtime.Input
{
    /// <summary>Calls <see cref="GameInputSystem.Tick"/> each frame. Attach to a scene object or bootstrap from game entry.</summary>
    public sealed class GameInputUpdater : MonoBehaviour
    {
        GameInputSystem _system;

        public void Install(GameInputSystem system) => _system = system;

        void Update() => _system?.Tick();
    }
}
