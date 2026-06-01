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
        private static readonly PlayerLoopSystem TimerSystem = new()
        {
            type = typeof(TimerManager),
            updateDelegate = TimerManager.UpdateTimers,
            subSystemList = null
        };

        static TimerBootstrapperEditorHook()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;

            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.RemoveSystem<Update>(ref currentPlayerLoop, in TimerSystem);
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            TimerManager.Clear();
        }
    }
}
