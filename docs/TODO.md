# TODO — `com.air.unity-game-core`

**Last Updated:** 2026-06-03 · **Owner:** package maintainers · **Scope:** L1 runtime follow-ups on **existing** APIs (English)

> **User doc (Chinese):** [../TODO.zh-CN.md](../TODO.zh-CN.md)

> **Boundary:** `GameRuntime`, `EventBus`, `IResManager`, entities, procedures host, timers, pools, scene/save/audio glue.  
> **Out of scope:** UI, CLI commands, reimplementing L0 command/entity/FSM rules.  
> **Meta rollup:** [AirUnityPackage `docs/TODO_ROADMAP.md`](https://github.com/Airuxul/AirUnityPackage/blob/main/docs/TODO_ROADMAP.md)

## Capability baseline

- `GameRuntime` / `IGameRuntime` explicit instance + `Current`
- `EventBus`, `UnityResManager` / AB loader, refcounted `UnloadRes`
- Input → `CommandHistory` via `InputPipeline`
- `TimerService` + PlayerLoop `TimerBootstrapper`
- `PoolRegistry`, `UnityEntityManager`, thin `ProcedureManager`, `SceneFlow`, `FileSaveService`, `AudioService`

## TODO

| ID | Pri | Title | Description |
|----|-----|-------|-------------|
| UGC-01 | P0 | Resource teardown on shutdown | `Dispose()` does not drain loads / `AssetBundleResManager.Clear()`; `UnloadRes` unused in package. |
| UGC-02 | P0 | Entity async load leak | Hide before `LoadInstanceAsync` callback must destroy orphaned `GameObject`. |
| UGC-03 | P0 | Entity ↔ resource pairing | Store prefab path per entity; call `UnloadRes` when views destroyed. |
| UGC-04 | P1 | Asset vs instance refcount split | Shared `ResLoadInfo` conflates asset and instance loads on one path key. |
| UGC-05 | P1 | Failed / pending load callbacks | Invoke or error-channel waiters when load fails (status → `Unload`). |
| UGC-06 | P1 | Use `IPoolRegistry` for entity views | `UnityEntityManager` injects pools but always `LoadInstanceAsync` + `Destroy`. |
| UGC-07 | P1 | Procedure tick bootstrap | `ProcedureManager.Tick` never called; add PlayerLoop/updater hook. |
| UGC-08 | P1 | Scene load safety | Failed-load events; cancel in-flight; coordinate `Single` load with entity teardown. |
| UGC-09 | P2 | Complete `GameRuntime.Dispose` | Procedures, input updaters, scene guard, optional `Current` handoff. |
| UGC-10 | P2 | Timer bootstrap contract | Document/install tick with runtime create; align editor exit vs `Dispose`. |
| UGC-11 | P2 | Input pipeline semantics | Held-key `Performed` every frame; consider `CanExecute` / chord policy. |
| UGC-12 | P2 | README / API drift | Remove or implement `RegisterDefaultEntitySystems`, `SpawnView`, `CreateQuery` mentions. |
| UGC-13 | P2 | Scene event surface | `LoadFailed` / progress for async consumers. |
| UGC-14 | P3 | `EventBus` hardening | Mixed handler types on same key; reentrancy during `Emit`. |
| UGC-15 | P3 | Pool registry key stability | Prefab instance ID invalidation on domain reload. |

## Do not assign here

| Topic | Owner package |
|-------|----------------|
| `ICommand` / undo stack logic | `com.air.game-core` |
| `EntityManager` / `ProcedureBase` rules | `com.air.game-core` |
| UI panels, triggers, navigator | `com.air.unity-ui` |
| CLI/HTTP commands | `com.air.unity-connector` |
| Game-specific procedure states | Game / sample project |
