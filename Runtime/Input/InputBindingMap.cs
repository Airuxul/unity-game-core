using System;
using System.Collections.Generic;
using Air.GameCore.Command;

namespace Air.UnityGameCore.Runtime.Input
{
    /// <summary>Maps logical input action ids to command factories.</summary>
    public sealed class InputBindingMap
    {
        readonly Dictionary<(string actionId, InputPhase phase), Func<ICommand>> _bindings =
            new();

        public void Bind(string actionId, InputPhase phase, Func<ICommand> factory)
        {
            if (string.IsNullOrEmpty(actionId))
                throw new ArgumentException("Action id is required.", nameof(actionId));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _bindings[(actionId, phase)] = factory;
        }

        public bool TryGet(string actionId, InputPhase phase, out Func<ICommand> factory) =>
            _bindings.TryGetValue((actionId, phase), out factory);
    }
}
