using Air.GameCore.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Air.UnityGameCore.Runtime.Serialization
{
    public static class JsonSerializationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterSubsystem() => EnsureRegistered();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterRuntime() => EnsureRegistered();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void RegisterEditor() => EnsureRegistered();
#endif

        public static void EnsureRegistered()
        {
            if (JsonSerialization.Instance == null)
                JsonSerialization.Instance = NewtonsoftJsonSerializer.Default;
            if (JsonHost.Instance == null)
                JsonHost.Instance = JsonSerialization.Instance;
        }
    }
}
