# GameFramework — WebRequest 模块解析

> 来源：[GameFramework](https://gameframework.cn/) by Jiang Yin (© 2013-2021)  
> 命名空间：`GameFramework.WebRequest`  
> 本文档由 GitHub Copilot (Claude Sonnet 4.6) 自动生成，用于快速理解该模块架构。

---

## 一、模块定位

WebRequest 模块是 GameFramework 中负责**网络 HTTP/HTTPS 请求**的统一管理层。它不直接调用 `UnityWebRequest`，而是通过**代理辅助器（Helper）**模式将"框架层任务调度"与"Unity 引擎网络实现"解耦，外部可自由替换底层请求实现（UnityWebRequest、HttpClient、第三方库等）。

---

## 二、文件结构速览

| 文件 | 职责 |
|------|------|
| `IWebRequestManager.cs` | 对外公开的管理器接口，定义全部 API |
| `WebRequestManager.cs` | 管理器核心实现，持有 `TaskPool<WebRequestTask>` |
| `WebRequestManager.WebRequestAgent.cs` | 单个请求代理，包装 Helper，处理超时计时 |
| `WebRequestManager.WebRequestTask.cs` | 单个请求任务数据容器（URI、PostData、超时时长） |
| `WebRequestManager.WebRequestTaskStatus.cs` | 任务状态枚举：`Todo / Doing / Done / Error` |
| `IWebRequestAgentHelper.cs` | 辅助器抽象接口，Unity 端实现此接口发出真实请求 |
| `WebRequestAgentHelperCompleteEventArgs.cs` | 辅助器→代理：请求完成事件（携带响应字节） |
| `WebRequestAgentHelperErrorEventArgs.cs` | 辅助器→代理：请求出错事件（携带错误描述） |
| `WebRequestStartEventArgs.cs` | 管理器→业务层：请求开始事件 |
| `WebRequestSuccessEventArgs.cs` | 管理器→业务层：请求成功事件（携带 `byte[]` 响应） |
| `WebRequestFailureEventArgs.cs` | 管理器→业务层：请求失败事件（携带错误消息） |
| `Constant.cs` | 内部常量：`DefaultPriority = 0` |

---

## 三、核心架构

```
业务层 (Game Logic)
    │  AddWebRequest(uri, postData, tag, priority, userData)
    ▼
IWebRequestManager
    │
    ▼
WebRequestManager  ──── TaskPool<WebRequestTask>
    │                        │
    │                 优先级队列排队
    │                        │
    ├── WebRequestAgent[0]  ←─ 从队列取任务
    ├── WebRequestAgent[1]       │
    └── WebRequestAgent[N]   调用 Helper
                                 │
                        IWebRequestAgentHelper
                        (Unity 端实现，封装 UnityWebRequest)
                                 │
                        ┌────────┴────────┐
                   Complete Event     Error Event
                        │                │
                   回调到 Agent      回调到 Agent
                        │
              WebRequestSuccess / WebRequestFailure 事件
                        │
                    业务层回调
```

### 关键设计点

1. **TaskPool + Agent 池**：可配置多个 Agent 并发处理请求（类似线程池），空闲 Agent 自动从等待队列中取下一个任务。
2. **优先级队列**：`AddWebRequest` 可传入 `priority`，高优先级请求优先被分配到空闲 Agent。
3. **超时检测**：`WebRequestAgent.Update()` 每帧累加 `WaitTime`，超过 `Timeout` 后主动触发错误事件，避免请求永久挂起。
4. **引用池（ReferencePool）**：所有 EventArgs 通过 `ReferencePool.Acquire/Release` 管理，减少 GC 压力，在高频请求场景中尤为重要。
5. **Tag 支持**：每个任务可携带字符串 Tag，便于批量查询同一类型请求的状态（如 `GetWebRequestInfos("login")`）。
6. **双向解耦**：Helper 接口只有 `Request(uri, userData)` / `Request(uri, postData, userData)` / `Reset()` 三个方法，框架层与 Unity API 完全隔离。

---

## 四、核心 API 速查

```csharp
// 注册 Agent（初始化时调用）
webRequestManager.AddWebRequestAgentHelper(helper);

// 发送 GET 请求（返回 serialId）
int id = webRequestManager.AddWebRequest("https://api.example.com/data");

// 发送 POST 请求
byte[] body = Encoding.UTF8.GetBytes("{\"key\":\"value\"}");
int id = webRequestManager.AddWebRequest("https://api.example.com/post", body, userData: myContext);

// 带优先级和 Tag
int id = webRequestManager.AddWebRequest(uri, postData, tag: "battle", priority: 10, userData: null);

// 监听事件
webRequestManager.WebRequestStart   += OnStart;
webRequestManager.WebRequestSuccess += OnSuccess;
webRequestManager.WebRequestFailure += OnFailure;

// 回调示例
void OnSuccess(object sender, WebRequestSuccessEventArgs e)
{
    string json = Encoding.UTF8.GetString(e.GetWebResponseBytes());
    // 处理响应...
}

// 取消请求
webRequestManager.RemoveWebRequest(serialId);

// 查询状态
TaskInfo info = webRequestManager.GetWebRequestInfo(serialId);
```

---

## 五、Unity 端 Helper 实现要点

Unity 侧需继承 `MonoBehaviour` 并实现 `IWebRequestAgentHelper`：

```csharp
public class UnityWebRequestAgentHelper : MonoBehaviour, IWebRequestAgentHelper
{
    public event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete;
    public event EventHandler<WebRequestAgentHelperErrorEventArgs>   WebRequestAgentHelperError;

    public void Request(string uri, object userData)
    {
        StartCoroutine(SendGet(uri, userData));
    }

    public void Request(string uri, byte[] postData, object userData)
    {
        StartCoroutine(SendPost(uri, postData, userData));
    }

    private IEnumerator SendGet(string uri, object userData)
    {
        using var req = UnityWebRequest.Get(uri);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            WebRequestAgentHelperComplete?.Invoke(this,
                WebRequestAgentHelperCompleteEventArgs.Create(req.downloadHandler.data));
        else
            WebRequestAgentHelperError?.Invoke(this,
                WebRequestAgentHelperErrorEventArgs.Create(req.error));
    }

    public void Reset() { StopAllCoroutines(); }
}
```

---

## 六、行业应用场景

### 6.1 游戏行业典型用途

| 场景 | 说明 |
|------|------|
| **登录 / 鉴权** | POST 账密或 Token，获取会话票据 |
| **排行榜 / 公告** | GET 服务器实时数据，定期刷新 |
| **热更新版本检查** | GET manifest.json，比对本地版本决定是否触发更新 |
| **AB 包 CDN 下载** | 配合 DownloadManager 使用，WebRequest 负责元数据查询 |
| **数据上报 / 埋点** | POST 游戏事件日志到数据平台 |
| **商城 / 活动数据** | 动态拉取商品列表、限时活动配置 |
| **好友 / 社交** | 查询好友列表、发送礼物等轻量 REST 调用 |
| **战斗结算同步** | 局后将结果 POST 到后端，拿到奖励数据 |

### 6.2 非游戏 Unity 项目（如机器人/工业 App）

| 场景 | 说明 |
|------|------|
| **设备状态轮询** | GET 机器人/传感器当前状态 JSON |
| **远程指令下发** | POST 控制指令到 REST API 网关 |
| **固件版本检查** | 检查 OTA 更新包版本号 |
| **日志上传** | 定期 POST 运行日志到云端 |

---

## 七、与同类方案的对比

| 方案 | 优点 | 缺点 |
|------|------|------|
| **GameFramework WebRequest** | 并发池、优先级、GC 优化、解耦 Helper | 需要手写 Helper；纯回调事件模式，不支持 async/await |
| 裸 `UnityWebRequest` + Coroutine | 简单直接 | 无并发管理、无优先级、GC 压力大 |
| UniTask + `UnityWebRequest` | async/await 体验好、无 GC | 需引入第三方库，不在框架内统一管理 |
| `HttpClient` (.NET) | 跨平台、功能强 | 不在 Unity 主线程，需手动回调切换 |

---

## 八、Copilot 快速记忆索引

> 以下是为下次快速理解代码预留的关键记忆点：

- **入口**：`IWebRequestManager.AddWebRequest(...)` 有 **16 个重载**，最终都归并到 `AddWebRequest(uri, postData, tag, priority, userData)` 五参数版本。
- **并发核心**：`TaskPool<WebRequestTask>` — 在 `WebRequestManager.cs` 中构造，`AddWebRequestAgentHelper()` 每调用一次就增加一个并发槽。
- **超时位置**：`WebRequestAgent.Update()` 中用 `m_WaitTime` 累加 `realElapseSeconds`，超限时伪造一个 Error 事件。
- **响应数据**：`WebRequestSuccessEventArgs.GetWebResponseBytes()` 返回 `byte[]`，业务层自行解码（UTF-8 JSON、Protobuf 等）。
- **取消请求**：`RemoveWebRequest(serialId)` 或 `RemoveWebRequests(tag)` — 通过序列号或标签批量取消。
- **Helper 生命周期**：Helper 的 `Reset()` 在每次任务完成/失败后由 Agent 调用，用于中止协程或清理状态。
- **ReferencePool**：所有 EventArgs（6 个类）都实现了 `IReference`，通过 `ReferencePool.Acquire/Release` 零分配复用。
