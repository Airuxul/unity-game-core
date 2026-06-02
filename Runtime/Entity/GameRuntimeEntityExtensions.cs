using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Entity
{
    public static class GameRuntimeEntityExtensions
    {
        public static EntityUpdater InstallEntityUpdater(this GameRuntime runtime)
        {
            var go = new GameObject("EntityUpdater");
            var updater = go.AddComponent<EntityUpdater>();
            if (runtime.Entities is UnityEntityManager manager)
                updater.Install(manager);
            Object.DontDestroyOnLoad(go);
            return updater;
        }
    }
}
