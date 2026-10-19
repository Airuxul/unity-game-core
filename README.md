# Unity Game Core (`com.air.unity-game-core`)

[简体中文](README.zh-CN.md)

**Layer:** L1 Unity infrastructure — depends on `com.air.game-core`. **No UI**, **no CLI/HTTP commands** (those live in `com.air.unity-ui` and `com.air.unity-connector`).

Unity runtime built around an explicit **`GameRuntime`** instance at the game entry point (not a global singleton facade). `GameRuntime.CreateDefault()` wires resources, input → undo stack, pools, JSON, entities, procedures, scenes, save, and audio. Global events and the time wheel live in `com.air.game-core`. `GameTimeDriver` advances real time and visual time from the PlayerLoop.

## Install

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

Requires `com.air.game-core` **3.0.0+** and `com.unity.nuget.newtonsoft-json`.

## `GameRuntime` surface

| Member | Role |
|--------|------|
| `Resources` | `IResManager` — sync/async asset loading |
| `InputBindings` / `InputPipeline` | Map input phases to `ICommand` → `CommandHistory` |
| `UndoStack` | GoF undo/redo (`com.air.game-core`) |
| `Pools` | `IPoolRegistry` — `GameObject` / `Component` pools |
| `Json` | `IJsonSerializer` (Newtonsoft); registers `JsonHost` on create |
| `Entities` | `IEntityManager` / `UnityEntityManager` — GF entities + GameObject views |
| `Procedures` | `ProcedureManager` — game lifecycle FSM |
| `Scenes` | `ISceneFlow` — load sync/async + scene events |
| `Save` | `ISaveService` — JSON save slots |
| `Audio` | `IAudioService` — BGM + pooled SFX |

`GameRuntime.Current` is set while the instance is active. Call `Dispose()` on shutdown to clear pools, entities, and audio.

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

`GameTimeDriver` calls `ViTimerInstance.Update` and `ViVisualTimerInstance.Update` every frame. Hold the node and `Detach` it when the owner closes.

```csharp
using Air.GameCore;

var node = new ViTimeNode1();
ViTimerInstance.SetTime(node, 3f, _ => Debug.Log("done"));
node.Detach();
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
using Air.GameCore;
using Air.UnityGameCore.Runtime.Event;

var sceneLoaded = new ViTupleCallback<string>();
EventManager.Instance.Attach(RuntimeEventId.SceneLoadCompleted, sceneLoaded, (id, name) => Debug.Log(name));
runtime.Scenes.Load("MainMenu");
sceneLoaded.End();
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
