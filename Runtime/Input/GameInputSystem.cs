using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Input
{
    /// <summary>Polls a <see cref="IUnityInputSource"/> and runs commands through <see cref="InputPipeline"/>.</summary>
    public sealed class GameInputSystem
    {
        readonly InputPipeline _pipeline;
        readonly IUnityInputSource _source;
        readonly List<InputEvent> _scratch = new();

        public GameInputSystem(InputPipeline pipeline, IUnityInputSource source)
        {
            _pipeline = pipeline ?? throw new System.ArgumentNullException(nameof(pipeline));
            _source = source ?? throw new System.ArgumentNullException(nameof(source));
        }

        public InputPipeline Pipeline => _pipeline;

        public void Tick()
        {
            _source.Collect(_scratch);
            _pipeline.ProcessBatch(_scratch);
            _scratch.Clear();
        }
    }
}
