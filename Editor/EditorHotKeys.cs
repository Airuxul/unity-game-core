using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Air.UnityGameCore.Editor
{
    public static class EditorHotKeys
    {
        static readonly MethodInfo flipLocked;
        static readonly PropertyInfo constrainProportions;
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        static EditorHotKeys()
        {
            // Cache static MethodInfo and PropertyInfo for performance
#if UNITY_2023_2_OR_NEWER
            var editorLockTrackerType = typeof(EditorGUIUtility).Assembly.GetType("UnityEditor.EditorGUIUtility+EditorLockTracker");
            flipLocked = editorLockTrackerType.GetMethod("FlipLocked", bindingFlags);
#endif
            constrainProportions = typeof(Transform).GetProperty("constrainProportionsScale", bindingFlags);
        }

        [MenuItem("Edit/Toggle Inspector Lock %l")]
        public static void Lock()
        {
#if UNITY_2023_2_OR_NEWER
            var inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

            foreach (var inspectorWindow in Resources.FindObjectsOfTypeAll(inspectorWindowType))
            {
                var lockTracker = inspectorWindowType.GetField("m_LockTracker", bindingFlags)?.GetValue(inspectorWindow);
                flipLocked?.Invoke(lockTracker, new object[] { });
            }
#else
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
#endif

            foreach (var activeEditor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (activeEditor.target is not Transform target) continue;

                var currentValue = (bool)constrainProportions.GetValue(target, null);
                constrainProportions.SetValue(target, !currentValue, null);
            }

            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        [MenuItem("Edit/Toggle Inspector Lock %l", true)]
        public static bool Valid()
        {
            return ActiveEditorTracker.sharedTracker.activeEditors.Length != 0;
        }

        [MenuItem("Tools/Load StartUp And Play %q")]
        public static void LoadStartUpAndPlay()
        {
            Debug.Log("LoadStartUpAndPlay");
            var scenePath = FindStartUpScenePath();
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("StartUp scene not found, cannot start Play Mode.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            if (!scene.IsValid())
            {
                Debug.LogError($"Failed to open StartUp scene, path: {scenePath}");
                return;
            }

            EditorApplication.isPlaying = true;
        }

        static string FindStartUpScenePath()
        {
            const string sceneName = "StartUp";
            var guids = AssetDatabase.FindAssets($"{sceneName} t:scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                {
                    return path;
                }
            }

            return null;
        }
    }
}
