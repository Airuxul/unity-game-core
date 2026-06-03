using System;
using Air.UnityGameCore.Runtime.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Air.UnityGameCore.Runtime.Scene
{
    public sealed class SceneFlow : ISceneFlow
    {
        readonly EventBus _events;

        public string ActiveSceneName => SceneManager.GetActiveScene().name;
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
                _events.Emit(SceneEvents.LoadCompleted, sceneName);
                onCompleted?.Invoke();
            };
        }
    }
}
