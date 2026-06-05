using UnityEngine;
using Object = UnityEngine.Object;

namespace Air.UnityGameCore.Runtime.Procedure
{
    public static class GameRuntimeProcedureExtensions
    {
        public static ProcedureUpdater InstallProcedureUpdater(this GameRuntime runtime)
        {
            var go = new GameObject("ProcedureUpdater");
            var updater = go.AddComponent<ProcedureUpdater>();
            updater.Install(runtime.Procedures);
            Object.DontDestroyOnLoad(go);
            return updater;
        }
    }
}
