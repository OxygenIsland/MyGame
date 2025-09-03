using System.Collections.Generic;

namespace StarWorld
{
    /// <summary>
    /// 消息监听
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// 需要监听的消息
        /// </summary>
        List<MsgID> MsgList { get; }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundle"></param>
        void HandMessage(MsgID id, Bundle bundle);
    }
}