using System;
using System.Collections.Generic;
using System.Threading;
using Stark.Core.Logs;
using UnityEngine;

namespace StarWorld.Common
{
    public class UnityLogAppender : ILogAppender
    {
        private LogMessageType level = LogMessageType.MSG_INFO;

        // 主线程ID缓存
        private static int s_MainThreadId = -1;

        // 检查是否在主线程
        private static bool IsMainThread()
        {
            if (s_MainThreadId == -1)
            {
                s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            }
            return Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
        }

        // 初始化主线程ID
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeMainThreadId()
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public LogMessageType Level
        {
            get { return level; }
            set { level = value; }
        }

        public void Write(LogInfo info)
        {
            if (info.MessageFlag >= level)
            {
                if (info.Parameter == null)
                {
                    InternalWriteLine(info.MessageFlag, info.Format);
                }
                else
                {
                    InternalWriteLine(info.MessageFlag, info.Format, info.Parameter);
                }
            }
        }

        private static void InternalWriteLine(LogMessageType messageFlag, string strFormat)
        {
            string text = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = "";

            switch (messageFlag)
            {
                case LogMessageType.MSG_NONE:
                    logMessage = $"[{text}][NONE]: {strFormat}";
                    break;
                case LogMessageType.MSG_INFO:
                    logMessage = $"[{text}][INFO]: {strFormat}";
                    break;
                case LogMessageType.MSG_WARNING:
                    logMessage = $"[{text}][WARNING]: {strFormat}";
                    break;
                case LogMessageType.MSG_DEBUG:
                    logMessage = $"[{text}][DEBUG]: {strFormat}";
                    break;
                case LogMessageType.MSG_ERROR:
                    logMessage = $"[{text}][ERROR]: {strFormat}";
                    break;
                case LogMessageType.MSG_FATALERROR:
                    logMessage = $"[{text}][FATAL ERROR]: {strFormat}";
                    break;
            }

            LogNoStack(logMessage, messageFlag);
        }

#if UNITY_ANDROID && !UNITY_EDITOR && !LOG_FULL_STACK
        private static AndroidJavaClass logClass = new AndroidJavaClass("android.util.Log");
#endif

        // 线程安全的日志输出
        public static void LogNoStack(string msg, LogMessageType messageFlag)
        {
#if UNITY_ANDROID && !UNITY_EDITOR && !LOG_FULL_STACK
            // 检查是否在主线程
            if (IsMainThread())
            {
                // 在主线程中直接调用，根据日志级别选择不同的Android日志方法
                switch (messageFlag)
                {
                    case LogMessageType.MSG_ERROR:
                    case LogMessageType.MSG_FATALERROR:
                    case LogMessageType.MSG_WARNING:
                        // warning以上需要打印出栈信息
                        LogToUnity(msg, messageFlag);
                        break;
                    case LogMessageType.MSG_DEBUG:
                        logClass.CallStatic<int>("d", "[Unity]", msg);
                        break;
                    default:
                        logClass.CallStatic<int>("i", "[Unity]", msg);
                        break;
                }
            }
            else
            {
                // 不在主线程，使用Debug
                LogToUnity(msg, messageFlag);
            }
#else
            LogToUnity(msg, messageFlag); // 编辑器下正常输出
#endif
        }

        // 根据日志级别输出到Debug
        private static void LogToUnity(string msg, LogMessageType messageFlag)
        {
            switch (messageFlag)
            {
                case LogMessageType.MSG_ERROR:
                case LogMessageType.MSG_FATALERROR:
                    Debug.LogError(msg);
                    break;
                case LogMessageType.MSG_WARNING:
                    Debug.LogWarning(msg);
                    break;
                default:
                    Debug.Log(msg);
                    break;
            }
        }

        private static void InternalWriteLine(
            LogMessageType messageFlag,
            string strFormat,
            params object[] arg
        )
        {
            try
            {
                // 检查参数是否为空或无效
                if (arg == null || arg.Length == 0)
                {
                    InternalWriteLine(messageFlag, strFormat);
                    return;
                }

                // 直接传递参数数组给 string.Format，它会自动处理
                string formattedMessage = string.Format(strFormat, arg);
                InternalWriteLine(messageFlag, formattedMessage);
            }
            catch (FormatException ex)
            {
                // 如果格式化失败，输出原始格式和参数信息
                string errorMsg =
                    $"Format error: '{strFormat}' with {arg?.Length ?? 0} parameters. Error: {ex.Message}";
                InternalWriteLine(messageFlag, errorMsg);
            }
            catch (Exception ex)
            {
                // 捕获其他异常
                string errorMsg = $"Unexpected error in log formatting: {ex.Message}";
                InternalWriteLine(messageFlag, errorMsg);
            }
        }
    }
}
