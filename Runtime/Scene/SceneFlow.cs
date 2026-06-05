using System;
using Air.UnityGameCore.Runtime.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Air.UnityGameCore.Runtime.Scene
{
    public sealed class SceneFlow : ISceneFlow
    {
        readonly EventBus _events;

        public string ActiveSceneName => ResolveActiveGameplaySceneName();
        public bool IsLoading { get; private set; }

        public SceneFlow(EventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is required.", nameof(sceneName));

            IsLoading = true;
            _events.Emit(SceneEvents.LoadStarted, sceneName);
            SceneManager.LoadScene(sceneName, mode);
            IsLoading = false;
            TrySetActiveGameplayScene(sceneName);
            _events.Emit(SceneEvents.LoadCompleted, sceneName);
            onCompleted?.Invoke();
        }

        public void LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is required.", nameof(sceneName));

            IsLoading = true;
            _events.Emit(SceneEvents.LoadStarted, sceneName);
            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            if (op == null)
            {
                IsLoading = false;
                Debug.LogError($"Failed to load scene: {sceneName}");
                return;
            }

            op.completed += _ =>
            {
                IsLoading = false;
                TrySetActiveGameplayScene(sceneName);
                _events.Emit(SceneEvents.LoadCompleted, sceneName);
                onCompleted?.Invoke();
            };
        }

        static string ResolveActiveGameplaySceneName()
        {
            var active = SceneManager.GetActiveScene();
            if (IsGameplayScene(active))
                return active.name;

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (IsGameplayScene(scene))
                    return scene.name;
            }

            return active.name;
        }

        static bool IsGameplayScene(UnityEngine.SceneManagement.Scene scene) =>
            scene.IsValid() && scene.isLoaded && !string.IsNullOrEmpty(scene.path);

        static void TrySetActiveGameplayScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!IsGameplayScene(scene) || scene.name != sceneName)
                    continue;

                if (SceneManager.GetActiveScene() == scene)
                    return;

                try
                {
                    SceneManager.SetActiveScene(scene);
                }
                catch (System.ArgumentException ex)
                {
                    Debug.LogWarning($"[SceneFlow] Skip SetActiveScene for '{sceneName}': {ex.Message}");
                }

                return;
            }
        }
    }
}
