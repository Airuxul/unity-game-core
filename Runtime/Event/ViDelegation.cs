using System;

namespace Air.UnityGameCore.Runtime.Event
{
    public sealed class ViDelegation
    {
        Action _action;

        public void Bind(Action handler)
        {
            if (handler != null)
                _action += handler;
        }

        public void Invoke() => _action?.Invoke();

        public void End() => _action = null;
    }
}
