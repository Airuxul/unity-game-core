# Unity Game Core（`com.air.unity-game-core`）

[English](README.md)

**层级：** L1 Unity 基建，依赖 `com.air.game-core`。**不含 UI**，**不含 CLI/HTTP 命令**（分别在 `com.air.unity-ui` 与 `com.air.unity-connector`）。

在游戏入口创建并持有 **`GameRuntime`** 实例（非全局单例门面）。`GameRuntime.CreateDefault()` 聚合事件、资源、输入→撤销栈、定时器、对象池、JSON、实体、流程、场景、存档与音频。

## 安装

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

需要 `com.air.game-core` **3.0.0+** 与 `com.unity.nuget.newtonsoft-json`。

## 能力概览

| 成员 | 作用 |
|------|------|
| `Events` | `EventBus` |
| `Resources` | 资源加载 |
| `InputBindings` / `InputPipeline` | 输入绑定到 `CommandHistory` |
| `UndoStack` | 撤销/重做（来自 game-core） |
| `Timers` / `Pools` | 定时器与 Unity 对象池 |
| `Json` | Newtonsoft 序列化；创建时注册 `JsonHost` |
| `Entities` | GF 实体 + GameObject 视图 |
| `Procedures` | 游戏生命周期状态机 |
| `Scenes` / `Save` / `Audio` | 场景流、存档槽、BGM/SFX |

退出时调用 `Dispose()` 释放定时器、池、实体、事件与音频，并清除 `GameRuntime.Current`。

输入、定时器、实体、场景等完整代码示例见 [English README](README.md)。

## 相关

- [Game Core](../com.air.game-core/README.md)
- [Unity UI](../com.air.unity-ui/README.md)
- [Unity Connector](../unity-cli/com.air.unity-connector/README.zh-CN.md)
