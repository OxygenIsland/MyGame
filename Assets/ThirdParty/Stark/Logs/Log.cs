using System;
using System.Collections.Generic;

namespace Stark.Core.Logs
{
    // public enum LogMessageType
    // {
    //     MSG_NONE,
    //     MSG_INFO,
    //     MSG_DEBUG,
    //     MSG_WARNING,
    //     MSG_ERROR,
    //     MSG_FATALERROR,
    // }

    // public struct LogInfo
    // {
    //     public LogMessageType MessageFlag { get; set; }
    //     public string Format { get; set; }
    //     public object[] Parameter { get; set; }
    // }

    // public interface ILogAppender
    // {
    //     LogMessageType Level { get; set; }
    //     void Write(LogInfo info);
    // }

    /// <summary>
    /// 日志管理器 - 统一管理日志输出
    /// </summary>
    public static class Log
    {
        private static readonly List<ILogAppender> appenders = new List<ILogAppender>();
        private static LogMessageType currentLogLevel = LogMessageType.MSG_INFO;

        /// <summary>
        /// 添加日志输出器
        /// </summary>
        /// <param name="appender">日志输出器</param>
        public static void AddAppender(ILogAppender appender)
        {
            if (appender == null)
                return;

            if (!appenders.Contains(appender))
            {
                appenders.Add(appender);
                appender.Level = currentLogLevel;
                UnityEngine.Debug.Log($"[LogManager] AddAppender: {appender.GetType().Name}");
            }
        }

        /// <summary>
        /// 移除日志输出器
        /// </summary>
        /// <param name="appender">日志输出器</param>
        public static void RemoveAppender(ILogAppender appender)
        {
            if (appender != null && appenders.Contains(appender))
            {
                appenders.Remove(appender);
                UnityEngine.Debug.Log($"[LogManager] RemoveAppender: {appender.GetType().Name}");
            }
        }

        /// <summary>
        /// 设置日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public static void SetLogLevel(LogMessageType level)
        {
            currentLogLevel = level;
            foreach (var appender in appenders)
            {
                appender.Level = level;
            }
            UnityEngine.Debug.Log($"[LogManager] SetLogLevel: {level}");
        }

        /// <summary>
        /// 获取当前日志级别
        /// </summary>
        /// <returns>当前日志级别</returns>
        public static LogMessageType GetLogLevel()
        {
            return currentLogLevel;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="messageFlag">日志级别</param>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        private static void WriteLog(
            LogMessageType messageFlag,
            string format,
            params object[] args
        )
        {
            if (messageFlag < currentLogLevel)
                return;

            var logInfo = new LogInfo(messageFlag, format, args);

            foreach (var appender in appenders)
            {
                try
                {
                    appender.Write(logInfo);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(
                        $"[LogManager] appender: {appender.GetType().Name} write failed: {ex.Message}"
                    );
                }
            }
        }

        /// <summary>
        /// 写入信息日志
        /// </summary>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        public static void Info(string format, params object[] args)
        {
            WriteLog(LogMessageType.MSG_INFO, format, args);
        }

        /// <summary>
        /// 写入调试日志
        /// </summary>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        public static void Debug(string format, params object[] args)
        {
            WriteLog(LogMessageType.MSG_DEBUG, format, args);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        public static void Warn(string format, params object[] args)
        {
            WriteLog(LogMessageType.MSG_WARNING, format, args);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        public static void Error(string format, params object[] args)
        {
            WriteLog(LogMessageType.MSG_ERROR, format, args);
        }

        /// <summary>
        /// 写入致命错误日志
        /// </summary>
        /// <param name="format">日志格式</param>
        /// <param name="args">参数</param>
        public static void FatalError(string format, params object[] args)
        {
            WriteLog(LogMessageType.MSG_FATALERROR, format, args);
        }

        /// <summary>
        /// 获取所有日志输出器
        /// </summary>
        /// <returns>日志输出器列表</returns>
        public static List<ILogAppender> GetAppenders()
        {
            return new List<ILogAppender>(appenders);
        }

        /// <summary>
        /// 清空所有日志输出器
        /// </summary>
        public static void ClearAppenders()
        {
            appenders.Clear();
            UnityEngine.Debug.Log("[LogManager] ClearAppenders");
        }
    }
}
