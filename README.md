# Unity Game Core (`com.air.unity-game-core`) v2

Unity runtime infrastructure: **no UI**, **no global singleton facade**. Hold a `GameRuntime` instance at the game entry point.

## Runtime API (v2)

| Type | Role |
|------|------|
| `GameRuntime` / `IGameRuntime` | `Events` + `Resources` |
| `EventBus` | `On` / `Emit` / `Off` |
| `IResManager` | Resource loading |
| `TimerManager` / `Timer` | Timers |
| `PoolManager` | Unity `GameObject` / `Component` pools |

Depends on **`com.air.game-core`** for pure C# helpers (e.g. `ListPool<T>` in resource code).

## Install

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

Requires `com.air.game-core` 1.0.1+.

## Editor-only (not part of Runtime v2 API)

`Editor/AssetDependency/*` ? asset dependency analyzer windows and cache. Safe to ignore for player builds; not used by `GameRuntime`.

## Related

- [Game Core](../com.air.game-core/README.md)
- [Unity UI](../com.air.unity-ui/README.md)
