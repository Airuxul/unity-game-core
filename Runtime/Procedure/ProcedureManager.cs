using Air.GameCore.Procedure;

namespace Air.UnityGameCore.Runtime.Procedure
{
    public sealed class ProcedureManager
    {
        public ProcedureMachine Machine { get; } = new();

        public void Tick(float deltaTime) => Machine.Tick(deltaTime);
    }
}
