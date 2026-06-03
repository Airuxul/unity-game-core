using Air.GameCore.Command;
using UnityEngine;

namespace Air.UnityGameCore.Runtime.Input
{
    public static class GameRuntimeInputExtensions
    {
        public const string UndoAction = "command.undo";
        public const string RedoAction = "command.redo";

        public static GameInputSystem BindUndoRedo(
            this GameRuntime runtime,
            UnityLegacyKeyboardSource source = null,
            KeyCode undoKey = KeyCode.Z,
            KeyCode redoKey = KeyCode.Y,
            bool ctrl = true)
        {
            if (runtime == null)
                throw new System.ArgumentNullException(nameof(runtime));

            source ??= new UnityLegacyKeyboardSource();

            runtime.InputBindings.Bind(UndoAction, InputPhase.Started, () => new UndoInputCommand(runtime.UndoStack));
            runtime.InputBindings.Bind(RedoAction, InputPhase.Started, () => new RedoInputCommand(runtime.UndoStack));

            if (ctrl)
            {
                source.MapChord(KeyCode.LeftControl, undoKey, UndoAction);
                source.MapChord(KeyCode.RightControl, undoKey, UndoAction);
                source.MapChord(KeyCode.LeftControl, redoKey, RedoAction);
                source.MapChord(KeyCode.RightControl, redoKey, RedoAction);
            }
            else
            {
                source.MapKey(undoKey, UndoAction);
                source.MapKey(redoKey, RedoAction);
            }

            return runtime.CreateLegacyKeyboardInput(source);
        }

        sealed class UndoInputCommand : ICommand
        {
            readonly CommandHistory _history;
            public UndoInputCommand(CommandHistory history) => _history = history;
            public void Execute() => _history.Undo();
        }

        sealed class RedoInputCommand : ICommand
        {
            readonly CommandHistory _history;
            public RedoInputCommand(CommandHistory history) => _history = history;
            public void Execute() => _history.Redo();
        }
    }
}
