using System.Collections.Generic;
using Air.GameCore.Command;

namespace Air.UnityGameCore.Runtime.Input
{
    /// <summary>Runs bound commands through a <see cref="CommandHistory"/>.</summary>
    public sealed class InputPipeline
    {
        readonly InputBindingMap _bindings;
        readonly CommandHistory _history;

        public InputPipeline(InputBindingMap bindings, CommandHistory history)
        {
            _bindings = bindings ?? throw new System.ArgumentNullException(nameof(bindings));
            _history = history ?? throw new System.ArgumentNullException(nameof(history));
        }

        public InputBindingMap Bindings => _bindings;
        public CommandHistory History => _history;

        public void ProcessBatch(IReadOnlyList<InputEvent> events)
        {
            if (events == null)
                return;

            foreach (var e in events)
            {
                if (!_bindings.TryGet(e.ActionId, e.Phase, out var factory))
                    continue;

                _history.Run(factory());
            }
        }
    }
}
