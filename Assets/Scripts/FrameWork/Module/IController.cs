using System.Collections;
using System.Collections.Generic;

namespace StarWorld.FrameWork
{
    public interface IController
    {
        List<MsgID> MsgList { get; }

        void HandMessage(MsgID id, Bundle bundle);

        IEnumerator OnStart();

        IEnumerator OnStop();
    }
}
