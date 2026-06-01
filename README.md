# Unity Game Core (`com.air.unity-game-core`) v2

Unity ????????**? UI????????**?

## API

| ?? | ?? |
|------|------|
| `GameRuntime` | `Events` + `Resources`?`GameRuntime.CreateDefault()` |
| `IGameRuntime` | ????? |
| `EventBus` | `On` / `Emit` / `Off` |
| `IResManager` | ?????`UnityResManager`?`AssetBundleResManager`? |
| `PoolManager` / `UnityObjectPool<T>` | Unity ??? |

## ??

```csharp
using Air.UnityGameCore.Runtime;

var runtime = GameRuntime.CreateDefault();
runtime.Events.On("game.start", () => { });
runtime.Events.Emit("game.start");
```

## ??

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

???`com.air.game-core` 1.0.1+

## ???

- [Game Core](../com.air.game-core/README.md)
- [unity-ui](../unity-ui/README.md)??? UI ??
