using UnityEngine;

namespace Air.UnityGameCore.Runtime.Procedure
{
    /// <summary>
    /// Drives <see cref="ProcedureManager.Tick"/> each frame while installed.
    /// </summary>
    public sealed class ProcedureUpdater : MonoBehaviour
    {
        ProcedureManager _procedures;

        public void Install(ProcedureManager procedures) => _procedures = procedures;

        void Update()
        {
            if (_procedures != null)
                _procedures.Tick(UnityEngine.Time.deltaTime);
        }
    }
}
