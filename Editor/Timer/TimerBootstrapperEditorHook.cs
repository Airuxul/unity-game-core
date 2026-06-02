using Air.UnityGameCore.Runtime;
using Air.UnityGameCore.Runtime.Time;
using Air.UnityGameCore.Runtime.Utils;
using UnityEditor;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Air.UnityGameCore.Editor
{
  [InitializeOnLoad]
  internal static class TimerBootstrapperEditorHook
  {
    static readonly PlayerLoopSystem TimerSystem = new()
    {
      type = typeof(TimerService),
      updateDelegate = () => GameRuntime.Current?.Timers?.UpdateTimers(),
      subSystemList = null
    };

    static TimerBootstrapperEditorHook()
    {
      EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
      if (state != PlayModeStateChange.ExitingPlayMode)
        return;

      var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
      PlayerLoopUtils.RemoveSystem<Update>(ref currentPlayerLoop, in TimerSystem);
      PlayerLoop.SetPlayerLoop(currentPlayerLoop);
      GameRuntime.Current?.Timers?.Clear();
    }
  }
}
