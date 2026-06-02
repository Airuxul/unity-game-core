# Unity Game Core (`com.air.unity-game-core`)

Unity runtime infrastructure: **no UI**, **no global singleton facade**. Hold a `GameRuntime` instance at the game entry point.

## Runtime API

| Type | Role |
|------|------|
| `GameRuntime` / `IGameRuntime` | `Events`, `Resources`, `Input`, `Undo`, `Timers`, `Pools`, `Json`, `Entities`, `Scenes`, `Save`, `Audio` |
| `InputPipeline` / `InputBindingMap` | Unity input ? `CommandHistory.Run` |
| `GameInputSystem` | Polls `IUnityInputSource`, drives `InputPipeline` each frame |
| `IJsonSerializer` / `JsonSerialization` | JSON contract + Newtonsoft |
| `EventBus` | `On` / `Emit` / `Off` |
| `IResManager` | Resource loading |
| `ITimerService` / `TimerService` | Timers via `runtime.Timers`; pass service into `CountdownTimer` etc. |
| `IPoolRegistry` / `PoolRegistry` | Unity `GameObject` / `Component` pools via `runtime.Pools` |
| `IEntityManager` / `UnityEntityManager` | GF entities + GameObject binding |
| `ProcedureManager` | Game lifecycle FSM |
| `ISceneFlow` / `SceneFlow` | Scene load sync/async + events |
| `ISaveService` / `FileSaveService` | JSON save slots |
| `IAudioService` / `AudioService` | BGM + pooled SFX |
| `GameRuntimeInputExtensions` | Ctrl+Z / Ctrl+Y ? `CommandHistory` |

Depends on **`com.air.game-core`** 2.4.0+ for Command and Entity.

CLI / HTTP commands are in **`com.air.unity-connector`** only.

### Input example

```csharp
using Air.GameCore.Command;
using Air.UnityGameCore.Runtime;
using Air.UnityGameCore.Runtime.Input;
using UnityEngine;

var runtime = GameRuntime.CreateDefault();

runtime.InputBindings.Bind("Jump", InputPhase.Started, () => new JumpCommand());

var keyboard = new UnityLegacyKeyboardSource();
keyboard.MapKey(KeyCode.Space, "Jump");
var input = runtime.CreateLegacyKeyboardInput(keyboard);
new GameObject("GameInput").AddComponent<GameInputUpdater>().Install(input);
```

### Timer example

```csharp
var runtime = GameRuntime.CreateDefault();
var cooldown = new CountdownTimer(3f, runtime.Timers);
cooldown.OnTimerStop += () => Debug.Log("done");
cooldown.Start();
```

### Entity example

```csharp
using Air.UnityGameCore.Runtime.Entity;
using Air.UnityGameCore.Runtime.Entity.Components;

runtime.RegisterDefaultEntitySystems();
runtime.InstallEntityUpdater();

var view = runtime.Entities.SpawnView(unitPrefab, Vector3.zero, Quaternion.identity, (id, host) =>
{
  host.AddComponent(id, new TransformComponent());
  host.AddComponent(id, new LifetimeComponent { Seconds = 30f });
});

// Multi-component query (reuse EntityQuery to avoid alloc)
var query = runtime.Entities.CreateQuery();
foreach (var id in query.With<TransformComponent>().With<LifetimeComponent>().Execute())
  { /* ... */ }
```

### Scene load

```csharp
runtime.Scenes.LoadAsync("MainMenu", onCompleted: () => Debug.Log("ready"));
runtime.Events.On<string>(SceneEvents.LoadCompleted, name => Debug.Log(name));
```

### Save / audio

```csharp
runtime.Save.Save("slot1", new PlayerData { Level = 3 });
runtime.Save.TryLoad("slot1", out PlayerData data);

runtime.Audio.PlayBgm(menuBgm);
runtime.Audio.PlaySfx(clickSfx);
```

### Undo / redo input

```csharp
var input = runtime.BindUndoRedo();
new GameObject("Input").AddComponent<GameInputUpdater>().Install(input);
```

### Shutdown

```csharp
using var runtime = GameRuntime.CreateDefault();
// ...
runtime.Dispose(); // timers, pools, events; clears GameRuntime.Current
```

## Install

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

Requires `com.air.game-core` 2.3.0+ and `com.unity.nuget.newtonsoft-json`.

## Related

- [Game Core](../com.air.game-core/README.md)
- [Unity Connector](../unity-cli/com.air.unity-connector/README.md)
