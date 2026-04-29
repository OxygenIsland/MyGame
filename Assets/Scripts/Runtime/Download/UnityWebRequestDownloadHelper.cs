using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Download;
using NLog;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 基于 UnityWebRequest + UniTask 实现的下载代理辅助器。
    /// 支持断点续传（HTTP Range）、64KB 流式写盘与主动取消。
    /// 挂载到 GameObject 上后，通过 IDownloadManager.AddDownloadAgentHelper 注册。
    /// </summary>
    public sealed class UnityWebRequestDownloadHelper : MonoBehaviour, IDownloadAgentHelper
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        private UnityWebRequest _webRequest;
        private CancellationTokenSource _cts;

        /// <summary>下载代理辅助器更新数据流事件。</summary>
        public event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes;

        /// <summary>下载代理辅助器更新数据大小事件。</summary>
        public event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;

        /// <summary>下载代理辅助器完成事件。</summary>
        public event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;

        /// <summary>下载代理辅助器错误事件。</summary>
        public event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据（全新下载）。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void Download(string downloadUri, object userData)
        {
            StartDownloadAsync(downloadUri, 0L, -1L, userData).Forget();
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据（断点续传）。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置（字节）。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void Download(string downloadUri, long fromPosition, object userData)
        {
            StartDownloadAsync(downloadUri, fromPosition, -1L, userData).Forget();
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据（指定字节范围）。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置（字节）。</param>
        /// <param name="toPosition">下载数据结束位置（字节）。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            StartDownloadAsync(downloadUri, fromPosition, toPosition, userData).Forget();
        }

        /// <summary>
        /// 重置下载代理辅助器，取消当前正在进行的下载并释放网络资源。
        /// 由框架在任务完成、失败或任务池重置时自动调用。
        /// </summary>
        public void Reset()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_webRequest != null)
            {
                _webRequest.Abort();
                _webRequest.Dispose();
                _webRequest = null;
            }
        }

        private void OnDestroy()
        {
            Reset();
        }

        private async UniTaskVoid StartDownloadAsync(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            _cts = new CancellationTokenSource();

            var streamHandler = new StreamingDownloadHandler(this);
            _webRequest = new UnityWebRequest(downloadUri, UnityWebRequest.kHttpVerbGET, streamHandler, null);

            // 断点续传：通过 HTTP Range 头指定起始字节
            if (fromPosition > 0L)
            {
                string range = toPosition > 0L
                    ? $"bytes={fromPosition}-{toPosition}"
                    : $"bytes={fromPosition}-";
                _webRequest.SetRequestHeader("Range", range);
            }

            try
            {
                await _webRequest.SendWebRequest().ToUniTask(cancellationToken: _cts.Token);

                if (_webRequest.result == UnityWebRequest.Result.Success)
                {
                    // e.Length 须为本次会话下载的字节数，与 DownloadAgent 中 m_DownloadedLength 对齐
                    var completeArgs = DownloadAgentHelperCompleteEventArgs.Create((long)_webRequest.downloadedBytes);
                    DownloadAgentHelperComplete?.Invoke(this, completeArgs);
                    ReferencePool.Release(completeArgs);
                }
                else
                {
                    Log.Warn($"下载失败: {_webRequest.error} | URL: {downloadUri}");
                    // false = 保留 .download 临时文件，以便下次断点续传
                    var errorArgs = DownloadAgentHelperErrorEventArgs.Create(false, _webRequest.error);
                    DownloadAgentHelperError?.Invoke(this, errorArgs);
                    ReferencePool.Release(errorArgs);
                }
            }
            catch (OperationCanceledException)
            {
                // 由 Reset() 主动取消，属于正常流程，不触发错误事件
            }
            catch (Exception ex)
            {
                Log.Error($"下载异常: {ex.Message} | URL: {downloadUri}");
                var errorArgs = DownloadAgentHelperErrorEventArgs.Create(false, ex.Message);
                DownloadAgentHelperError?.Invoke(this, errorArgs);
                ReferencePool.Release(errorArgs);
            }
        }

        /// <summary>
        /// 流式数据接收处理器。
        /// 使用 64KB 预分配缓冲区，每收到一个数据块立即同步通知 DownloadAgent 写盘，
        /// 避免将完整文件内容缓存到内存后再统一写出。
        /// </summary>
        private sealed class StreamingDownloadHandler : DownloadHandlerScript
        {
            private const int BufferSize = 64 * 1024; // 64 KB

            private readonly UnityWebRequestDownloadHelper _owner;

            public StreamingDownloadHandler(UnityWebRequestDownloadHelper owner)
                : base(new byte[BufferSize])
            {
                _owner = owner;
            }

            /// <summary>
            /// Unity 每收到一个数据块时回调。data 为预分配缓冲区（被复用），
            /// 必须在方法返回前同步消费，不可缓存引用。
            /// </summary>
            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || dataLength <= 0)
                    return true;

                // 通知 DownloadAgent 将数据写入磁盘（FileStream.Write 在事件处理器内同步执行）
                var bytesArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(data, 0, dataLength);
                _owner.DownloadAgentHelperUpdateBytes?.Invoke(_owner, bytesArgs);
                ReferencePool.Release(bytesArgs);

                // 通知 DownloadCounter 更新实时速度统计
                var lenArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(dataLength);
                _owner.DownloadAgentHelperUpdateLength?.Invoke(_owner, lenArgs);
                ReferencePool.Release(lenArgs);

                return true;
            }
        }
    }
}
