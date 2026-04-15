using System;
using System.Threading;
using UnityEngine;
using NLog;

/// <summary>
/// 请求上下文管理，提供一个全局可访问的 TraceId，用于日志关联和调试
/// </summary>
public static class TraceContext
{
    // AsyncLocal 在 async/await 中自动向下流转，不会污染兄弟任务
    private static readonly AsyncLocal<string> _traceId = new();

    public static string Current
    {
        get => _traceId.Value ?? "no-trace";
        private set
        {
            _traceId.Value = value;

            // 使用 NLog 5.0+ 推荐的 ScopeContext.PushProperty 替代 MDLC
            // 需要管理 property 的生命周期，避免泄漏
            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null;
            }
            if (value != null)
            {
                _scope = NLog.ScopeContext.PushProperty("traceId", value);
            }
        }
    }


    // ScopeContext property 句柄
    private static IDisposable _scope;

    // 在请求入口调用，返回一个 scope 对象，using 结束自动清理
    public static TraceScope Begin(string traceId = null)
    {
        var id = traceId ?? TraceIdGenerator.New();
        var previous = Current;
        Current = id;
        return new TraceScope(previous);
    }

    // 给协程专用：手动绑定一个 traceId 到当前线程
    public static void Bind(string traceId) => Current = traceId;
    public static void Clear() => Current = null;
}

// RAII 风格的 scope，using 块结束时自动恢复上层 traceId
public readonly struct TraceScope : IDisposable
{
    private readonly string _previous;
    public TraceScope(string previous) => _previous = previous;
    public void Dispose() => TraceContext.Bind(_previous);
}

public static class TraceIdGenerator
{
    private static int _counter;
    // 格式：设备尾4位-时间戳-序号，便于跨端对齐
    public static string New() =>
        $"{SystemInfo.deviceUniqueIdentifier[^4..]}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Interlocked.Increment(ref _counter)}";
}