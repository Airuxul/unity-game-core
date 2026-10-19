using Air.GameCore;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Air.UnityGameCore.Runtime.Utils;

namespace Air.UnityGameCore.Runtime.Time
{
    public static class GameTimeDriver
    {
        static bool _installed;
        static RealTimeBudget _realTime;
        static PlayerLoopSystem _updateSystem;
        static PlayerLoopSystem _flushSystem;

        // Player-loop entries are identified by type. These two must not share GameTimeDriver.
        sealed class RealTimeTick { }
        sealed class EventFlush { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Initialize()
        {
            ViTimerLog.Exception = Debug.LogException;
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            _updateSystem = new PlayerLoopSystem
            {
                type = typeof(RealTimeTick),
                updateDelegate = Tick,
                subSystemList = null
            };
            _flushSystem = new PlayerLoopSystem
            {
                type = typeof(EventFlush),
                updateDelegate = FlushEvents,
                subSystemList = null
            };

            var updateInstalled = PlayerLoopUtils.InsertSystem<Update>(ref loop, in _updateSystem, 0);
            var flushInstalled = PlayerLoopUtils.InsertSystem<PreLateUpdate>(ref loop, in _flushSystem, 0);
            if (!updateInstalled || !flushInstalled)
            {
                Debug.LogWarning("GameTimeDriver failed to register into the player loop.");
                return;
            }

            PlayerLoop.SetPlayerLoop(loop);
            _installed = true;
        }

        public static void Shutdown()
        {
            if (_installed)
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopUtils.RemoveSystem<Update>(ref loop, in _updateSystem);
                PlayerLoopUtils.RemoveSystem<PreLateUpdate>(ref loop, in _flushSystem);
                PlayerLoop.SetPlayerLoop(loop);
                _installed = false;
            }

            ViTimerInstance.Clear();
            ViVisualTimerInstance.Clear();
            EventManager.Instance.Reset();
            _realTime = default;
        }

        static void Tick()
        {
            ViTimerInstance.Update(_realTime.Consume(UnityEngine.Time.unscaledDeltaTime));
            ViVisualTimerInstance.Update(UnityEngine.Time.deltaTime);
        }

        static void FlushEvents() => EventManager.Instance.Flush();
    }
}
