using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using ErrorOr;
using NLog;
using UnityEngine;

public class SdkApiCoroutine : MonoBehaviour
{
    private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
    // ── 对外接口：返回 IEnumerator，前端用 StartCoroutine 调用 ──
    public IEnumerator Login(string uid, string token, Action<ErrorOr<LoginData>> callback)
    {
        // ToCoroutine() 把 UniTask 异步流包成协程
        // 内部用 UniTask 处理网络，外部保持协程接口不变
        yield return LoginUniTask(uid, token, callback).ToCoroutine();
    }

    // ── 内部实现：UniTask async，协程驱动 ──
    private async UniTask LoginUniTask(string uid, string token,
                                        Action<ErrorOr<LoginData>> callback)
    {
        // 1. 创建上下文，Bind traceId 到当前线程
        var ctx = new RequestContext("/user/login");
        Log.Info("SdkApi", $"Login coroutine start | uid={uid}");

        // 2. 参数校验
        if (string.IsNullOrEmpty(uid))
        {
            Log.Warn("SdkApi", "Invalid params | uid empty");
            callback(Error.Failure("0001", "uid is empty"));
            return;
        }

        // 3. 使用 UnityWebRequest（协程模式通常配合 UWR，保持和旧代码一致）
        var uwr = UnityEngine.Networking.UnityWebRequest.Post(
            "https://api.example.com/user/login",
            $"{{\"uid\":\"{uid}\"}}");
        uwr.SetRequestHeader("Content-Type", "application/json");

        // 4. await UWR —— UniTask 封装的版本，等待完成
        //    注意：这里会触发调度器切换，AsyncLocal 可能丢失
        await uwr.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());

        // 5. !! 恢复点：每次 await 之后必须重新 Bind !!  ctx 对象没有丢，traceId 还在，只是 AsyncLocal 需要重新绑
        ctx.Rebind();
        Log.Info("Network", $"Response received | elapsed={ctx.ElapsedMs}ms");

        if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Log.Error("SdkApi", $"Network error | {uwr.error} | code={uwr.responseCode}");
            callback(Error.Failure("", uwr.error));
            return;
        }

        var raw = uwr.downloadHandler.text;
        Log.Info("Network", $"Raw body={raw}");

        // 6. 嵌套子请求示例：登录成功后拉取用户信息
        //    用 SpawnChild 生成子 span，日志里能看出父子关系
        var profileCtx = ctx.SpawnChild("/user/profile");
        await FetchProfileUniTask(profileCtx);

        // 7. 子请求完成后，恢复父级 traceId
        ctx.Rebind();
        Log.Info("SdkApi", $"Login flow complete | elapsed={ctx.ElapsedMs}ms");

        try
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginData>(raw);
            callback(data);
        }
        catch (Exception ex)
        {
            Log.Error("SdkApi", "Parse failed", ex);
            callback(Error.Failure("", ex.Message));
        }
    }

    // ── 子请求（同样是 UniTask，可以无限嵌套）──
    private async UniTask FetchProfileUniTask(RequestContext ctx)
    {
        Log.Info("Profile", $"Fetch start");   // 日志会打出 trace-xxx.s1

        var uwr = UnityEngine.Networking.UnityWebRequest.Get(
            "https://api.example.com/user/profile");
        await uwr.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());

        ctx.Rebind();   // 子请求恢复点同样需要 Rebind
        Log.Info("Profile", $"Fetch done | elapsed={ctx.ElapsedMs}ms");
    }
}