using Cysharp.Threading.Tasks;
using ErrorOr;
using NLog;
using System;
using System.Net.Http;
using System.Threading;

public class SdkApiAsync
{
    private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _http = new();

    // ── 对外接口：主线程调用，主线程回调 ──
    public void Login(string uid, string token, Action<ErrorOr<LoginData>> callback)
    {
        // UniTask.Void 让 async 流程在后台跑，不阻塞调用方
        LoginAsync(uid, token, callback).Forget();
    }

    // ── 内部实现：全程 async/await ──
    private async UniTaskVoid LoginAsync(string uid, string token, Action<ErrorOr<LoginData>> callback)
    {
        // 1. 主线程入口，生成 traceId，此时 AsyncLocal 绑定完毕
        using var scope = TraceContext.Begin();
        var ctx = new RequestContext("/user/login");

        Log.Info("SdkApi", $"Login start | uid={uid}");

        // 2. 参数校验
        if (string.IsNullOrEmpty(uid))
        {
            Log.Warn("SdkApi", $"Invalid params | uid empty");
            // 返回错误描述，ErrorOr 支持丰富的错误信息，方便调用方处理
            callback(Error.Validation("0001", "UID cannot be empty"));
            return;
        }

        try
        {
            // 3. 切到线程池 —— 后续代码在子线程运行，AsyncLocal 的副本被自动携带过来，Current 仍然是同一个 traceId
            await UniTask.SwitchToThreadPool();

            Log.Info("Network", $"POST {ctx.Api} | thread={Thread.CurrentThread.ManagedThreadId}");

            // 4. 网络 I/O（子线程，不阻塞主线程帧循环）
            var body    = $"{{\"uid\":\"{uid}\",\"token\":\"{token}\"}}";
            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var resp    = await _http.PostAsync("https://api.example.com/user/login", content);
            var raw     = await resp.Content.ReadAsStringAsync();

            Log.Info("Network", $"Response {(int)resp.StatusCode} | elapsed={ctx.ElapsedMs}ms | body={raw}");

            if (!resp.IsSuccessStatusCode)
            {
                // 5a. 错误路径：切回主线程再回调（Unity 的回调里可能操作 GameObject）
                await UniTask.SwitchToMainThread();
                Log.Error("SdkApi", $"Server error | code={(int)resp.StatusCode}");
                callback(Error.Failure("0002", $"HTTP {(int)resp.StatusCode}"));
                return;
            }

            // 5b. JSON 解析也在子线程完成，大包体不卡帧
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginData>(raw);

            // 6. 切回主线程，安全操作 Unity API
            await UniTask.SwitchToMainThread();

            Log.Info("SdkApi", $"Login success | playerId={data.PlayerId} | elapsed={ctx.ElapsedMs}ms");
            callback(data);
        }
        catch (OperationCanceledException)
        {
            await UniTask.SwitchToMainThread();
            Log.Warn("SdkApi", "Login cancelled");
            callback(Error.Failure("0003", "Request was cancelled"));
        }
        catch (Exception ex)
        {
            await UniTask.SwitchToMainThread();
            Log.Error("SdkApi", "Login exception", ex);
            callback(Error.Failure("0004", ex.Message));
        }
    }
}

public class LoginData
{
    public object PlayerId { get; internal set; }
}