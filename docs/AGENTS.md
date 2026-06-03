# AGENTS — `com.air.unity-game-core`

**Last Updated:** 2026-06-02 · **Scope:** canonical agent entry (this repository)

## Package role

| Field | Value |
|-------|--------|
| Layer | **L1** Unity infrastructure |
| Depends on | `com.air.game-core` (Command, Entity, Serialization contracts) |
| Does **not** include | UI (`com.air.unity-ui`), CLI/HTTP commands (`com.air.unity-connector`) |
| Entry type | `GameRuntime` / `IGameRuntime` — create at game entry, `Dispose` on shutdown; no global singleton facade |

Indexed in meta repo [AirUnityPackage](https://github.com/Airuxul/AirUnityPackage) (`config/registry.json`, tag `unity-runtime`).

## User documentation

| File | Language |
|------|----------|
| [README.md](../README.md) | English |
| [README.zh-CN.md](../README.zh-CN.md) | Chinese |

## Agent documentation

| File | Purpose |
|------|---------|
| [AGENTS.md](AGENTS.md) | This file |
| [DOC_GOVERNANCE.md](DOC_GOVERNANCE.md) | Doc workflow for this repo |
| [CHANGELOG_AGENT.md](CHANGELOG_AGENT.md) | Agent change log |

## Runtime modules (technical map)

| Folder | Namespace | Responsibility |
|--------|-----------|----------------|
| `Runtime/Core/` | `Air.UnityGameCore.Runtime` | `GameRuntime`, `IGameRuntime` |
| `Runtime/Event/` | `...Event` | `EventBus` |
| `Runtime/Resource/` | `...Resource` | `IResManager`, loaders |
| `Runtime/Input/` | `...Input` | `InputPipeline`, `GameInputSystem`, bindings → `CommandHistory` |
| `Runtime/Time/` | `...Time` | `ITimerService`, `TimerService`, `TimerImpl/*` |
| `Runtime/Pool/` | `...Pool` | `IPoolRegistry`, Unity object pools |
| `Runtime/Entity/` | `...Entity` | `UnityEntityManager`, view binding |
| `Runtime/Procedure/` | `...Procedure` | `ProcedureManager` (game lifecycle FSM) |
| `Runtime/Scene/` | `...Scene` | `ISceneFlow`, `SceneFlow` |
| `Runtime/Save/` | `...Save` | `ISaveService`, `FileSaveService` |
| `Runtime/Audio/` | `...Audio` | `IAudioService`, `AudioService` |
| `Runtime/Serialization/` | `...Serialization` | Newtonsoft `IJsonSerializer`, `JsonSerializationBootstrap` |
| `Runtime/Coroutine/` | `...Coroutine` | `WaitFor` helpers |
| `Runtime/Utils/` | `...Utils` | `PlayerLoopUtils` |
| `Editor/` | `Air.UnityGameCore.Editor.*` | Asset dependency tools, editor extensions |

GoF command and GF entity design live in **`com.air.game-core`**: [Command/DESIGN.md](https://github.com/Airuxul/game-core/blob/main/Command/DESIGN.md), [Entity/DESIGN.md](https://github.com/Airuxul/game-core/blob/main/Entity/DESIGN.md).

## Required reads before doc updates

1. `docs/AGENTS.md`
2. `docs/DOC_GOVERNANCE.md`
3. `README.md`, `README.zh-CN.md`

## Meta repository standards

When editing C# or layers, also follow:

- [ARCHITECTURE](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
- [CONSTRAINTS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_CONSTRAINTS.md)
- [C_SHARP_STANDARDS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/C_SHARP_STANDARDS.md) (§4.2 this package)

Doc skills (`doc-read-index`, `doc-generate-update`) run from the **meta repo** only — do not add `.cursor/skills/` under this package.
