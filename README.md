# MyGame

个人 Unity 工具库与框架示例工程。

---

## 项目结构 (`Assets/Scripts/`)

```
Scripts/
├── GameFramework/          # 纯 C# GF框架核心（无 Unity 依赖）
│   └── MyGame.GameFramework.asmdef
│
├── FrameWork/              # 自定义基础框架层（单例、消息、生命周期）
│   ├── Message/            # 消息总线 (MsgManager, MsgID, Bundle)
│   ├── Singleton/          # 单例模式 (MonoSingleton, Singleton)
│   ├── BaseObserver.cs     # 观察者基类
│   ├── LifeCycle.cs        # 生命周期接口
│   ├── MonoSingletonCreator.cs
│   ├── SingleTon.cs        # ⚠ 命名遗留问题，与 Singleton/Singleton.cs 命名空间不同
│   └── MyGame.Framework.asmdef
│
├── Runtime/                # Unity 运行时桥接层（VContainer 入口、游戏流程）
│   ├── Logs/               # NLog 日志管理
│   ├── Procedure/          # 游戏流程 (Launch → CheckVersion → Preload → Main)
│   ├── GameEntry.cs        # Bootstrap MonoBehaviour
│   ├── GameAppEntryPoint.cs# 纯 C# 应用入口 (IStartable/ITickable/IDisposable)
│   └── MyGame.Runtime.asmdef
│
├── Scopes/                 # VContainer 依赖注入根容器
│   ├── RootLifetimeScope.cs
│   └── MyGame.Scopes.asmdef
│
└── Toolkit/                # 工具库（按功能分类）
    ├── Collections/        # 数据结构 (CircularBuffer, DropoutStack, FixedSizeQueue…)
    ├── Coroutine/          # 协程工具 (CoroutineHelper, Loom, InternalUpdator…)
    ├── Extensions/         # 扩展方法 (ComponentExtension, ExpandFunction)
    ├── IO/                 # 文件读写 (FileTool, Persistence)
    ├── Math/               # 数学工具 (MathsTool, QuaternionUtilities)
    ├── Misc/               # 杂项 (EncryptDecipherTool, Helper, Tools…)
    ├── Network/            # 网络工具 (WebService)
    ├── Pool/               # 对象池 (IPool, Pool, UnityObjectPool…)
    ├── Resources/          # 资源工具 (ResourcesManager, ResourcesUtil)
    ├── Task/               # 异步/任务 (DelayAction, DelayTask, TaskLite…)
    ├── Time/               # 时间工具 (FTimer, TimeStampHelper, TimestampTool)
    ├── UI/                 # UI 工具 (FrameSelection, ShowFPS, SequenceFramePlayer…)
    ├── _Archive/           # 废弃代码存档（不参与编译）
    └── MyGame.Toolkit.asmdef
```

## Assembly 依赖关系

```
MyGame.GameFramework  (纯 C#，noEngineReferences)
        ↑
MyGame.Framework      (Unity + NLog → GameFramework)
MyGame.Runtime        (Unity + VContainer → GameFramework)
        ↑
MyGame.Scopes         (VContainer → GameFramework + Runtime)

MyGame.Toolkit        (Unity，独立，无框架依赖)
```

## 主要第三方依赖

| 包 | 用途 |
|---|---|
| VContainer | 依赖注入容器 |
| UniTask | 异步编程 |
| R3 | 响应式扩展 |
| NLog | 结构化日志（via NuGet） |
| GameFramework | 游戏模块管理框架 |
