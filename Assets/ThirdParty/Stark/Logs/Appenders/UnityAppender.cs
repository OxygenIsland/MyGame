using System;
using UnityEngine;

namespace Stark.Core.Logs
{
    /// <summary>
    /// Unity Debug日志输出
    /// </summary>
    /// <remarks>
    /// 虽然这里实现的是实例的接口
    /// 但控制台只有一个，因此这边的数据都是写入一个静态的列表
    /// </remarks>
    public class UnityAppender : ILogAppender
    {
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

            if (info.Parameter == null)
                InternalWriteLine(info.MessageFlag, info.Format);
            else
                InternalWriteLine(info.MessageFlag, info.Format, info.Parameter);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFlag"></param>
        /// <param name="strFormat"></param>
        static void InternalWriteLine(LogMessageType messageFlag, string strFormat)
        {
            var time_stamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            switch (messageFlag)
            {
                case LogMessageType.MSG_NONE: // direct printf replacement
                    Debug.Log($"[{time_stamp}][NONE]: {strFormat}");
                    break;
                case LogMessageType.MSG_STATUS:
                    Debug.Log($"[{time_stamp}][STATUS]: {strFormat}");
                    break;
                case LogMessageType.MSG_SQL:
                    Debug.Log($"[{time_stamp}][SQL]: {strFormat}");
                    break;
                case LogMessageType.MSG_INFO:
                    Debug.Log($"[{time_stamp}][INFO]: {strFormat}");
                    break;
                case LogMessageType.MSG_NOTICE:
                    Debug.Log($"[{time_stamp}][NOTICE]: {strFormat}");
                    break;
                case LogMessageType.MSG_WARNING:
                    Debug.LogWarning($"[{time_stamp}][WARNING]: {strFormat}");
                    break;
                case LogMessageType.MSG_DEBUG:
                    Debug.Log($"[{time_stamp}][DEBUG]: {strFormat}");
                    break;
                case LogMessageType.MSG_ERROR:
                    Debug.LogError($"[{time_stamp}][ERROR]: {strFormat}");
                    break;
                case LogMessageType.MSG_FATALERROR:
                    Debug.LogError($"[{time_stamp}][FATAL ERROR]: {strFormat}");
                    break;
                case LogMessageType.MSG_HACK:
                    Debug.Log($"[{time_stamp}][HACK]: {strFormat}");
                    break;
                case LogMessageType.MSG_LOAD:
                    Debug.Log($"[{time_stamp}][LOAD]: {strFormat}");
                    break;
                case LogMessageType.MSG_DOS_PROMPT:
                    Debug.Log($"[{time_stamp}][DOS]: {strFormat}");
                    break;
                case LogMessageType.MSG_INPUT:
                    Debug.Log($"[{time_stamp}][INPUT]: {strFormat}");
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFlag"></param>
        /// <param name="strFormat"></param>
        /// <param name="arg"></param>
        static void InternalWriteLine(LogMessageType messageFlag, string strFormat, params object[] arg)
        {
            InternalWriteLine(messageFlag, string.Format(strFormat, arg));
        }

    }
}