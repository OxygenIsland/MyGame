using System.Linq;
using UnityEngine;

namespace StarWorld.FrameWork
{
    public class WaitForMsg : CustomYieldInstruction
    {
        private bool isDone = false;
        public override bool keepWaiting => !isDone;

        public void Done()
        {
            if (isDone)
            {
                return;
            }

            isDone = true;
            foreach (var msg in _msgList)
            {
                MsgManager.Instance.UnRegist(msg, HandMessage);
            }
        }

        private MsgID[] _msgList;

        public WaitForMsg(MsgID[] msgList)
        {
            _msgList = msgList;
            foreach (var msg in _msgList)
            {
                MsgManager.Instance.Regist(msg, HandMessage);
            }
        }

        public void HandMessage(MsgID id, Bundle bundle)
        {
            if (_msgList.Any(x => x == id))
            {
                Done();
            }
        }
    }
}

