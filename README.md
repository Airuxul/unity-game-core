# Unity Game Core (`com.air.unity-game-core`) v2

Unity 运行时基础设施：**无 UI、无全局单例门面**。

## API

| 类型 | 说明 |
|------|------|
| `GameRuntime` | `Events` + `Resources` |
| `EventBus` | `On` / `Emit` / `Off` |
| `IResManager` | 资源加载 |

## 安装

```json
"com.air.unity-game-core": "file:../CustomPackages/packages/com.air.unity-game-core"
```

依赖 `com.air.game-core` 1.0.1+。
