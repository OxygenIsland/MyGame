using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiBuild
{
    /// <summary>
    /// 外部进程执行结果
    /// </summary>
    public class ProcessResult
    {
        public int    ExitCode { get; }
        public string Output   { get; }
        public string Error    { get; }
        public bool   Success  => ExitCode == 0;

        public ProcessResult(int exitCode, string output, string error)
        {
            ExitCode = exitCode;
            Output   = output;
            Error    = error;
        }
    }

    public static class ShellHelper
    {
        // 默认超时 5 分钟，上传大文件时可通过参数延长
        private const int DefaultTimeoutMs = 5 * 60 * 1000;

        #region Python 快捷方法

        /// <summary>
        /// 异步执行 Python 脚本（非阻塞，结果通过回调返回）。
        /// 调用方无需自行包 Task.Run()。
        /// </summary>
        public static void RunPythonAsync(string args, Action<ProcessResult> onComplete = null, int timeoutMs = DefaultTimeoutMs)
        {
            string python = ResolvePythonPath();
            RunAsync(python, args, onComplete, timeoutMs);
        }

        /// <summary>
        /// 同步执行 Python 脚本并返回结果（适合 Editor 批处理）。
        /// 内部已处理 stdout/stderr 异步读取，不会死锁。
        /// </summary>
        public static ProcessResult RunPythonSync(string args, int timeoutMs = DefaultTimeoutMs)
        {
            string python = ResolvePythonPath();
            return RunSync(python, args, timeoutMs);
        }

        #endregion

        #region 通用进程执行

        /// <summary>
        /// 异步执行任意可执行文件。
        /// </summary>
        public static void RunAsync(string executable, string args, Action<ProcessResult> onComplete = null, int timeoutMs = DefaultTimeoutMs)
        {
            Task.Run(() =>
            {
                var result = RunSync(executable, args, timeoutMs);
                onComplete?.Invoke(result);
            });
        }

        /// <summary>
        /// 同步执行任意可执行文件，异步读取 stdout/stderr 以避免死锁。
        /// </summary>
        public static ProcessResult RunSync(string executable, string args, int timeoutMs = DefaultTimeoutMs)
        {
            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();

            var startInfo = new ProcessStartInfo
            {
                FileName               = executable,
                Arguments              = args,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
            };

            using (var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                // 用事件异步读取，避免缓冲区满导致的死锁
                process.OutputDataReceived += (_, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
                process.ErrorDataReceived  += (_, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };

                UnityEngine.Debug.Log($"[ShellHelper] 启动: {executable} {args}");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = process.WaitForExit(timeoutMs);

                if (!exited)
                {
                    try { process.Kill(); } catch { /* 忽略 Kill 异常 */ }
                    string timeoutMsg = $"[ShellHelper] 超时（{timeoutMs}ms），已强制终止: {executable}";
                    UnityEngine.Debug.LogError(timeoutMsg);
                    return new ProcessResult(-1, sbOut.ToString(), timeoutMsg);
                }

                // WaitForExit(timeout) 返回后，异步读取可能还未完成，需再次等待排空
                process.WaitForExit();

                var result = new ProcessResult(process.ExitCode, sbOut.ToString(), sbErr.ToString());
                LogResult(result, executable);
                return result;
            }
        }

        #endregion

        #region 私有工具

        private static void LogResult(ProcessResult result, string executable)
        {
            if (!string.IsNullOrWhiteSpace(result.Output))
                UnityEngine.Debug.Log($"[ShellHelper] stdout:\n{result.Output}");

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                // 有些工具把进度信息写到 stderr，不全是错误，用 Warning 而非 Error
                UnityEngine.Debug.LogWarning($"[ShellHelper] stderr:\n{result.Error}");
            }

            if (!result.Success)
                UnityEngine.Debug.LogError($"[ShellHelper] 进程退出码 {result.ExitCode}: {executable}");
            else
                UnityEngine.Debug.Log($"[ShellHelper] 执行成功: {executable}");
        }

        /// <summary>
        /// 按优先级查找 Python 解释器路径：
        /// 1. 环境变量 UNITY_PYTHON_PATH（CI 环境注入）
        /// 2. python3（Linux/macOS 标准）
        /// 3. python（Windows / 旧版）
        /// </summary>
        private static string ResolvePythonPath()
        {
            string envPath = System.Environment.GetEnvironmentVariable("UNITY_PYTHON_PATH");
            if (!string.IsNullOrEmpty(envPath))
                return envPath;

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return "python3";
#else
            return "python";
#endif
        }

        #endregion
    }
}
