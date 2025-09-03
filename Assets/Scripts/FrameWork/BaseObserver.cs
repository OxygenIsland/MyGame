using System.Collections.Generic;

namespace StarWorld.FrameWork
{
    public class BaseObserver : IMessageHandler, ILifeCycleObserver
    {
        public virtual List<MsgID> MsgList => new List<MsgID>();

        public virtual void HandMessage(MsgID id, Bundle bundle)
        {

        }

        public virtual void OnStart()
        {
            if (MsgList != null)
            {
                foreach (var id in MsgList)
                {
                    MsgManager.Instance.Regist(id, this.HandMessage);
                }
            }
        }

        public virtual void OnStop()
        {
            if (MsgList != null)
            {
                foreach (var id in MsgList)
                {
                    MsgManager.Instance.UnRegist(id, this.HandMessage);
                }
            }
        }
    }
}