# Unity Game Core (`com.air.unity-game-core`)

[简体中文](README.zh-CN.md)

**Layer:** L1 Unity infrastructure — depends on `com.air.game-core`. **No UI**, **no CLI/HTTP commands** (those live in `com.air.unity-ui` and `com.air.unity-connector`).

Unity runtime built around an explicit **`GameRuntime`** instance at the game entry point (not a global singleton facade). `GameRuntime.CreateDefault()` wires event bus, resources, input → undo stack, timers, pools, JSON, entities, procedures, scenes, save, and audio.

## Install

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

Requires `com.air.game-core` **3.0.0+** and `com.unity.nuget.newtonsoft-json`.

## `GameRuntime` surface

| Member | Role |
|--------|------|
| `Events` | `EventBus` — `On` / `Emit` / `Off` |
| `Resources` | `IResManager` — sync/async asset loading |
| `InputBindings` / `InputPipeline` | Map input phases to `ICommand` → `CommandHistory` |
| `UndoStack` | GoF undo/redo (`com.air.game-core`) |
| `Timers` | `ITimerService` — pass into `CountdownTimer` etc. |
| `Pools` | `IPoolRegistry` — `GameObject` / `Component` pools |
| `Json` | `IJsonSerializer` (Newtonsoft); registers `JsonHost` on create |
| `Entities` | `IEntityManager` / `UnityEntityManager` — GF entities + GameObject views |
| `Procedures` | `ProcedureManager` — game lifecycle FSM |
| `Scenes` | `ISceneFlow` — load sync/async + scene events |
| `Save` | `ISaveService` — JSON save slots |
| `Audio` | `IAudioService` — BGM + pooled SFX |

`GameRuntime.Current` is set while the instance is active. Call `Dispose()` on shutdown to clear timers, pools, entities, events, and audio.

## Examples

### Input → command

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

### Timer

```csharp
var runtime = GameRuntime.CreateDefault();
var cooldown = new CountdownTimer(3f, runtime.Timers);
cooldown.OnTimerStop += () => Debug.Log("done");
cooldown.Start();
```

### Entity

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
runtime.Dispose(); // clears GameRuntime.Current
```

## Related

- [Game Core](../com.air.game-core/README.md) — pure C# Command / Entity / Serialization
- [Unity UI](../com.air.unity-ui/README.md) — panels on top of `GameRuntime`
- [Unity Connector](../unity-cli/com.air.unity-connector/README.md) — CLI / HTTP invoke
