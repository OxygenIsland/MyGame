﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stark.Core.Logs
{
    /// <summary>
    /// web输出适配器
    /// </summary>
    public class WebAppender : ILogAppender
    {
        /// <summary>
        /// 当前需要处理的集合
        /// </summary>
        private static readonly Queue<LogInfo> s_LogInfoQueue = new Queue<LogInfo>();

        /// <summary>
        /// 日志操作的锁
        /// </summary>
        private static object s_LockLogInfoQueue = new object();

        /// <summary>
        /// （全局，线程安全）是否加锁
        /// </summary>
        private static volatile bool s_IsLock;

        #region IAppender 成员

        /// <summary>
        /// 写日志（可以多线程操作）
        /// </summary>
        /// <param name="info"></param>
        public void Write(LogInfo info)
        {
            //  日志过滤
            if (info.MessageFlag < level)
                return;

            bool bIsLock = false;

            lock (s_LockLogInfoQueue)
            {
                s_LogInfoQueue.Enqueue(info);

                // 检测是否有其它的线程已在处理中，如在使用就退出，否则开始锁定
                if (s_IsLock == false)
                    bIsLock = s_IsLock = true;
            }


            // 如果有其它的线程在处理就退出
            if (bIsLock == false)
                return;

            LogInfo[] logInfoArray;

            do
            {
                logInfoArray = null;

                lock (s_LockLogInfoQueue)
                {
                    if (s_LogInfoQueue.Count > 0)
                    {
                        logInfoArray = s_LogInfoQueue.ToArray();
                        s_LogInfoQueue.Clear();
                    }
                    else
                        s_IsLock = false; // 没有数据需要处理,释放锁定让其它的程序来继续处理
                }

                if (logInfoArray == null)
                    break;

                for (int iIndex = 0; iIndex < logInfoArray.Length; iIndex++)
                {
                    LogInfo logInfo = logInfoArray[iIndex];

                    if (logInfo.Parameter == null)
                        InternalWriteLine(logInfo.MessageFlag, logInfo.Format);
                    else
                        InternalWriteLine(logInfo.MessageFlag, logInfo.Format, logInfo.Parameter);
                }

            } while (true);
        }

        #endregion

        /// <summary>
        /// 日志等级
        /// </summary>
        private LogMessageType level = LogMessageType.MSG_INFO;

        /// <summary>
        /// 日志等级
        /// </summary>
        public LogMessageType Level
        {
            get { return level; }
            set { level = value; }
        }

        private string url;

        public WebAppender(string url)
        {
            this.url = url;
        }

        private void InternalWriteLine(LogMessageType messageFlag, string format, object[] parameter)
        {
            InternalWriteLine(messageFlag, string.Format(format, parameter));
        }

        private void InternalWriteLine(LogMessageType messageFlag, string format)
        {

        }

    }
}
