using System;
using UnityEngine.SceneManagement;

namespace Air.UnityGameCore.Runtime.Scene
{
    public interface ISceneFlow
    {
        string ActiveSceneName { get; }
        bool IsLoading { get; }

        void Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onCompleted = null);

        void LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onCompleted = null);
    }
}
