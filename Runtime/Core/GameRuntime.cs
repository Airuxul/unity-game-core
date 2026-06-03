using System;
using Air.GameCore.Command;
using Air.GameCore.Entity;
using Air.UnityGameCore.Runtime.Entity;
using Air.UnityGameCore.Runtime.Procedure;
using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Input;
using Air.UnityGameCore.Runtime.Pool;
using Air.UnityGameCore.Runtime.Resource;
using Air.UnityGameCore.Runtime.Audio;
using Air.UnityGameCore.Runtime.Save;
using Air.UnityGameCore.Runtime.Scene;
using Air.GameCore.Serialization;
using Air.UnityGameCore.Runtime.Serialization;
using Air.UnityGameCore.Runtime.Time;

namespace Air.UnityGameCore.Runtime
{
    /// <summary>
    /// Default game runtime. Create at entry, call <see cref="Dispose"/> on shutdown.
    /// Timers tick via PlayerLoop only while this instance is <see cref="Current"/>.
    /// </summary>
    public sealed class GameRuntime : IGameRuntime
    {
        public static GameRuntime Current { get; private set; }

        public EventBus Events { get; }
        public IResManager Resources { get; }
        public InputBindingMap InputBindings { get; }
        public InputPipeline InputPipeline { get; }
        public CommandHistory UndoStack { get; }
        public ITimerService Timers { get; }
        public IPoolRegistry Pools { get; }
        public IJsonSerializer Json { get; }
        public IEntityManager Entities { get; }
        public ProcedureManager Procedures { get; }
        public UnityEntityLogicFactory EntityLogics { get; }
        public DictionaryEntityAssetProvider EntityAssets { get; }
        public ISceneFlow Scenes { get; }
        public ISaveService Save { get; }
        public IAudioService Audio { get; }

        readonly AudioService _audioService = new();

        bool _disposed;

        public GameRuntime(
            IResManager resources,
            IJsonSerializer json = null,
            int undoStackDepth = 128)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            Events = new EventBus();
            InputBindings = new InputBindingMap();
            UndoStack = new CommandHistory(undoStackDepth);
            InputPipeline = new InputPipeline(InputBindings, UndoStack);
            Timers = new TimerService();
            Pools = new PoolRegistry();
            EntityLogics = new UnityEntityLogicFactory();
            EntityAssets = new DictionaryEntityAssetProvider();
            Entities = new UnityEntityManager(EntityLogics, Resources, EntityAssets, Events, Pools);
            Procedures = new ProcedureManager();
            Scenes = new SceneFlow(Events);

            JsonSerializationBootstrap.EnsureRegistered();
            Json = json ?? NewtonsoftJsonSerializer.Default;
            Save = new FileSaveService(Json);
            Audio = _audioService;

            Current = this;
        }

        public static GameRuntime CreateDefault() => new(new UnityResManager());

        public GameInputSystem CreateLegacyKeyboardInput(UnityLegacyKeyboardSource source = null)
        {
            source ??= new UnityLegacyKeyboardSource();
            return new GameInputSystem(InputPipeline, source);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Timers.Clear();
            Pools.ClearAll();
            Entities.HideAllEntities();
            Events.Clear();
            _audioService.Dispose();

            if (Current == this)
                Current = null;

            _disposed = true;
        }
    }
}
