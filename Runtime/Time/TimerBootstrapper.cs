using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Air.UnityGameCore.Runtime.Utils;

namespace Air.UnityGameCore.Runtime.Time {
    internal static class TimerBootstrapper {
        static PlayerLoopSystem timerSystem;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize() {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0)) {
                Debug.LogWarning("Improved Timers not initialized, unable to register TimerManager into the Update loop.");
                return;
            }
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index) {
            timerSystem = new PlayerLoopSystem() {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}
