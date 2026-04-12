using System;

public class RequestContext
{
    public string TraceId    { get; }
    public string Api        { get; set; }
    public long   StartTick  { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // 子 span 链路：一个请求触发了多个子请求时，可以追踪每一段
    private int _spanCounter;
    public string NewSpanId() => $"{TraceId}.s{++_spanCounter}";

    // 耗时计算
    public long ElapsedMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - StartTick;

    // 协程 yield 恢复后调用，把 traceId 重新绑定到当前线程
    public void Rebind() => TraceContext.Bind(TraceId);

    // 子请求创建子 span，继承父 traceId 作为前缀
    public RequestContext SpawnChild(string childApi) =>
        new($"{Api}→{childApi}", $"{TraceId}.s{++_spanCounter}");

    public RequestContext(string api, string traceId = null)
    {
        Api     = api;
        TraceId = traceId ?? TraceIdGenerator.New();

        // 同步绑定到 AsyncLocal，让 async/await 也能自动读到
        TraceContext.Bind(TraceId);
    }
}