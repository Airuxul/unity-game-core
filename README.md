# Unity Game Core (`unity-game-core`) v2

Unity 运行时基础设施�?*�?UI、无全局单例**�?
## API

| 类型 | 说明 |
|------|------|
| `GameRuntime` | `Events` + `Resources`，`GameRuntime.CreateDefault()` |
| `IGameRuntime` | 运行时契�?|
| `EventBus` | `On` / `Emit` / `Off` |
| `IResManager` | 资源加载（`UnityResManager`、`AssetBundleResManager`�?|
| `PoolManager` / `UnityObjectPool<T>` | Unity 对象�?|

## 示例

```csharp
using Air.UnityGameCore.Runtime;

var runtime = GameRuntime.CreateDefault();
runtime.Events.On("game.start", () => { });
runtime.Events.Emit("game.start");
```

## 安装

```json
"unity-game-core": "file:../CustomPackages/packages/unity-game-core"
```

依赖：`game-core` 1.0.1+

## �?UI 协作

�?[Air UI](../ui/README.md) �?`GameEntry.CreateWithUI()`�?