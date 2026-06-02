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
using Air.UnityGameCore.Runtime.Time;

namespace Air.UnityGameCore.Runtime
{
  public interface IGameRuntime : IDisposable
  {
    EventBus Events { get; }
    IResManager Resources { get; }
    InputBindingMap InputBindings { get; }
    InputPipeline InputPipeline { get; }
    CommandHistory UndoStack { get; }
    ITimerService Timers { get; }
    IPoolRegistry Pools { get; }
    IJsonSerializer Json { get; }
    IEntityManager Entities { get; }
    ProcedureManager Procedures { get; }
    ISceneFlow Scenes { get; }
    ISaveService Save { get; }
    IAudioService Audio { get; }
  }
}

