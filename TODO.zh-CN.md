# 待办 — `com.air.unity-game-core`

**最后更新：** 2026-06-03 · **范围：** L1 Unity 运行时现有 API 的后续优化（中文）

> **职责边界：** `GameRuntime`、`EventBus`、`IResManager`、Unity 实体、流程宿主、定时器、对象池、场景/存档/音频胶水。  
> **不负责：** UI、CLI 命令、在包内重写 L0 命令/实体/FSM 规则。  
> Agent 英文条目：[`docs/TODO.md`](docs/TODO.md)

## 现有能力概要

- `GameRuntime` / `IGameRuntime` 显式实例与 `Current`
- `EventBus`、资源管理器与 `UnloadRes` 引用计数
- 输入经 `InputPipeline` 进入 `CommandHistory`
- `TimerService` + PlayerLoop 引导
- `PoolRegistry`、`UnityEntityManager`、薄 `ProcedureManager`、`SceneFlow`、存档与音频

## 待办列表

| ID | 优先级 | 标题 | 说明 |
|----|--------|------|------|
| UGC-01 | P0 | 关闭时资源释放 | `Dispose()` 未清空加载表/AB；包内几乎未调用 `UnloadRes`。 |
| UGC-02 | P0 | 实体异步加载泄漏 | 回调前 Hide 须销毁已实例化的 `GameObject`。 |
| UGC-03 | P0 | 实体与资源路径配对 | 记录预制体路径，销毁视图时 `UnloadRes`。 |
| UGC-04 | P1 | 资源与实例引用计数分离 | 同一路径键混用 Asset/Instance 加载。 |
| UGC-05 | P1 | 加载失败回调 | 失败时通知或错误通道，避免等待方挂起。 |
| UGC-06 | P1 | 实体使用对象池 | 已注入 `IPoolRegistry` 但仍直接异步实例化+`Destroy`。 |
| UGC-07 | P1 | 流程 Tick 引导 | `ProcedureManager.Tick` 无 PlayerLoop/Updater 调用。 |
| UGC-08 | P1 | 场景加载安全 | 失败事件、取消进行中加载、`Single` 与实体清理协同。 |
| UGC-09 | P2 | 完善 `Dispose` | 流程、输入更新器、场景守卫、`Current` 切换策略。 |
| UGC-10 | P2 | 定时器引导约定 | 与创建/编辑器退出/`Dispose` 文档化一致。 |
| UGC-11 | P2 | 输入管线语义 | 长按每帧 `Performed`；可考虑 `CanExecute`/和弦策略。 |
| UGC-12 | P2 | README 与 API 一致 | 文档中的 `SpawnView`/`CreateQuery` 等待实现或删除。 |
| UGC-13 | P2 | 场景事件扩展 | `LoadFailed`/进度等。 |
| UGC-14 | P3 | `EventBus` 加固 | 同键混用委托类型；`Emit` 中 `Off` 重入。 |
| UGC-15 | P3 | 池注册表键稳定性 | 域重载后预制体 InstanceID 失效。 |

## 请勿在本包实现

| 主题 | 归属包 |
|------|--------|
| 命令/撤销栈逻辑 | `com.air.game-core` |
| 实体/流程基类规则 | `com.air.game-core` |
| UI、导航、触发器 | `com.air.unity-ui` |
| CLI/HTTP | `com.air.unity-connector` |
| 具体游戏流程状态 | 游戏工程 / 示例 |
