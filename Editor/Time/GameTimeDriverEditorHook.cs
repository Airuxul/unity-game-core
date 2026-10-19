using Air.UnityGameCore.Runtime.Time;
using UnityEditor;

namespace Air.UnityGameCore.Editor
{
    [InitializeOnLoad]
    static class GameTimeDriverEditorHook
    {
        static GameTimeDriverEditorHook()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                GameTimeDriver.Shutdown();
        }
    }
}
