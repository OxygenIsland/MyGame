/**
 * ==========================================
 * Author：xuzq9
 * CreatTime：2023.7.5
 * Description：Message regist and listen tool
 * ==========================================
 */

using System.Collections.Generic;
using StarWorld.FrameWork;
using UnityEngine;

namespace StarWorld
{
    public delegate void MessageHandler(MsgID id, Bundle bundle);

    /// <summary>
    /// 消息管理器
    /// step1：新建一个消息号
    /// step2：在相应模块注册需要订阅的消息（RegistMsg），此模块需继承IMsgHandle
    /// step3：在相应的位置发送消息（SendMsg）
    /// </summary>
    public class MsgManager : Singleton<MsgManager>
    {
        private Dictionary<MsgID, List<MessageHandler>> msgDict =
            new Dictionary<MsgID, List<MessageHandler>>();

        public MsgManager() { }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handle"></param>
        public void Regist(MsgID id, MessageHandler handle)
        {
            List<MessageHandler> handlist;
            if (msgDict.ContainsKey(id))
            {
                msgDict.TryGetValue(id, out handlist);
                if (handlist == null)
                {
                    handlist = new List<MessageHandler>();
                }
                handlist.Add(handle);
            }
            else
            {
                handlist = new List<MessageHandler>();
                handlist.Add(handle);
                msgDict.Add(id, handlist);
            }
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool UnRegist(MsgID id, MessageHandler handle)
        {
            bool result = false;
            if (msgDict.ContainsKey(id))
            {
                List<MessageHandler> handlelist;
                msgDict.TryGetValue(id, out handlelist);
                if (handlelist != null)
                {
                    for (int i = 0; i < handlelist.Count; i++)
                    {
                        if (handlelist[i] == handle)
                        {
                            handlelist.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="id"></param>
        public void SendMsg(MsgID id, Bundle bundle = null)
        {
            List<MessageHandler> handlist;
            if (msgDict.TryGetValue(id, out handlist))
            {
                if (handlist != null)
                {
                    for (int i = 0; i < handlist.Count; i++)
                    {
                        try
                        {
                            handlist[i](id, bundle);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"msg:{id.ToString()} Exception:{ex.ToString()}");
                            //throw;
                        }
                    }
                }
                else
                {
                    msgDict.Remove(id);
                    //Debug.Log("the hand list is null : " + id.ToString());
                }
            }
            else
            {
                msgDict.Remove(id);
                //Debug.Log("the hand list is null : " + id.ToString());
            }
        }
    }
}

// Example:
//public class TestScene : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
//        MsgManager.Instance.RegistMsg(MsgID.Test1, Test1Handler);
//        MsgManager.Instance.RegistMsg(MsgID.Test2, Test2Handler);
//        MsgManager.Instance.RegistMsg(MsgID.Test3, Test3Handler);
//    }

//    private void Test1Handler(MsgID id, Bundle bundle)
//    {
//        Debug.LogError(bundle.GetValue<int>("key"));
//    }

//    private void Test2Handler(MsgID id, Bundle bundle)
//    {
//        Debug.LogError(bundle.GetValue<string>("key"));
//    }

//    private void Test3Handler(MsgID id, Bundle bundle)
//    {
//        Debug.LogError(bundle.GetValue<float>("key"));
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A))
//        {
//            Bundle bundle = new Bundle();
//            bundle.SetValue("key", 12);
//            MsgManager.Instance.SendMsg(MsgID.Test1, bundle);
//        }

//        if (Input.GetKeyDown(KeyCode.B))
//        {
//            Bundle bundle = new Bundle();
//            bundle.SetValue("key", "hello");
//            MsgManager.Instance.SendMsg(MsgID.Test2, bundle);
//        }

//        if (Input.GetKeyDown(KeyCode.C))
//        {
//            Bundle bundle = new Bundle();
//            bundle.SetValue("key", 1.5f);
//            MsgManager.Instance.SendMsg(MsgID.Test3, bundle);
//        }

//        if (Input.GetKeyDown(KeyCode.D))
//        {
//            MsgManager.Instance.RemoveMsg(MsgID.Test1, Test1Handler);
//        }
//    }
//}
