using Air.UnityGameCore.Runtime;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Air.UnityGameCore.Runtime.Utils;

namespace Air.UnityGameCore.Runtime.Time
{
  internal static class TimerBootstrapper
  {
    static PlayerLoopSystem _timerSystem;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    internal static void Initialize()
    {
      var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

      if (!InsertTimerTick<Update>(ref currentPlayerLoop, 0))
      {
        Debug.LogWarning("Timers not initialized: unable to register into the Update loop.");
        return;
      }

      PlayerLoop.SetPlayerLoop(currentPlayerLoop);
    }

    static void TickActiveRuntimeTimers()
    {
      GameRuntime.Current?.Timers?.UpdateTimers();
    }

    static bool InsertTimerTick<T>(ref PlayerLoopSystem loop, int index)
    {
      _timerSystem = new PlayerLoopSystem
      {
        type = typeof(TimerService),
        updateDelegate = TickActiveRuntimeTimers,
        subSystemList = null
      };
      return PlayerLoopUtils.InsertSystem<T>(ref loop, in _timerSystem, index);
    }
  }
}
