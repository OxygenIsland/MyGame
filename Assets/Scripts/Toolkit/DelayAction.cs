using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public class DelayAction : MonoBehaviour
    {
        # region 单例
        private static DelayAction _instance;
        public static DelayAction Instance
        {
            get
            {
                if (_instance == null)
                {
                    var parentGo = GameObject.Find("Singleton");
                    if (parentGo == null)
                    {
                        parentGo = new GameObject("Singleton");
                        DontDestroyOnLoad(parentGo);
                    }
                    _instance = new GameObject(nameof(DelayAction)).AddComponent<DelayAction>();
                    _instance.transform.SetParent(parentGo.transform);
                }

                return _instance;
            }
        }
        #endregion

        #region 2组待调用的方法列表：根据是否使用真实时间做区分。调用完成后需立刻移除。
        private static List<DelayInvokeActionScaledTime> Actions { get; set; } = new List<DelayInvokeActionScaledTime>();

        private static Dictionary<DelayInvokeActionUnscaledTime, IEnumerator> UnscaledTimeActionsDict { get; set; } = new Dictionary<DelayInvokeActionUnscaledTime, IEnumerator>();
        #endregion

        [ContextMenu("清除")]
        public void Clear()
        {
            if (Actions != null && Actions.Count > 0)
            {
                Actions.Clear();
            }

            if (UnscaledTimeActionsDict != null && UnscaledTimeActionsDict.Count > 0)
            {
                foreach (var coroutine in UnscaledTimeActionsDict.Values)
                {
                    StopCoroutine(coroutine);
                }
                UnscaledTimeActionsDict.Clear();
            }
        }

        /// <summary>
        /// 延迟调用某方法
        /// </summary>
        /// <param name="delayTime">延迟多久调用（秒）</param>
        /// <param name="action"></param>
        /// <param name="unscaledTime">是否无视timeScale</param>
        /// <returns></returns>
        public DelayInvokeAction Invoke(float delayTime, Action action, bool unscaledTime = false)
        {
            DelayInvokeAction delayInvokeAction = null;
            if (action != null)
            {
                if (delayTime == 0)
                {
                    action?.Invoke();
                }
                else
                {
                    if (unscaledTime)
                    {
                        var temp = new DelayInvokeActionUnscaledTime(delayTime, action);
                        UnscaledTimeActionsDict.Add(temp, StartDelayInvokeActionCoroutine(temp));
                        delayInvokeAction = temp;
                    }
                    else
                    {
                        var temp = new DelayInvokeActionScaledTime(delayTime, action);
                        Actions.Add(temp);
                        delayInvokeAction = temp;
                    }
                }
            }

            return delayInvokeAction;
        }

        # region 取消和移除某方法
        private void Cancel(DelayInvokeAction action)
        {
            if (action != null)
            {
                Remove(action);
            }
        }

        private void Remove(DelayInvokeAction action)
        {
            if (action != null)
            {
                if (action is DelayInvokeActionScaledTime)
                {
                    Remove(action as DelayInvokeActionScaledTime);
                }
                else if (action is DelayInvokeActionUnscaledTime)
                {
                    Remove(action as DelayInvokeActionUnscaledTime);
                }
            }
        }

        private void Remove(DelayInvokeActionScaledTime action)
        {
            Actions.Remove(action);
        }
        private void Remove(DelayInvokeActionUnscaledTime action)
        {
            if (UnscaledTimeActionsDict.ContainsKey(action))
            {
                StopCoroutine(UnscaledTimeActionsDict[action]);
                UnscaledTimeActionsDict.Remove(action);
            }
        }
        #endregion


        #region 等待一段时间后调用某方法：在Update中不断判断
        /// <summary>
        /// 每帧判断要延迟调用的各方法是否已到达调用时机
        /// </summary>
        private void Update()
        {
            List<int> hasInvokedActionsIndexList = new List<int>();
            for (int i = 0; i < Actions.Count; i++)
            {
                var curAction = Actions[i];

                if (CanInvoke(curAction))
                {
                    curAction.Invoke();
                    hasInvokedActionsIndexList.Add(i);
                }
            }

            if (hasInvokedActionsIndexList.Count > 0)
            {
                for (int i = hasInvokedActionsIndexList.Count - 1; i >= 0; i--)
                {
                    if (Actions.Count > hasInvokedActionsIndexList[i])
                    {
                        Remove(Actions[hasInvokedActionsIndexList[i]]);
                    }
                }
            }

            bool CanInvoke(DelayInvokeActionScaledTime action)
            {
                bool canInvoke = action.InvokeTime <= Time.time;
                return canInvoke;
            }
        }
        #endregion

        # region 等待真实的时间后调用方法：通过协程实现
        private static IEnumerator StartDelayInvokeActionCoroutine(DelayInvokeActionUnscaledTime action)
        {
            var unscaledAction = UnscaledTimeActionCoroutine(action);
            Instance.StartCoroutine(unscaledAction);
            return unscaledAction;
        }

        private static IEnumerator UnscaledTimeActionCoroutine(DelayInvokeActionUnscaledTime action)
        {
            yield return new WaitForSecondsRealtime(action.DelayTime);
            action?.Invoke();
            Instance.Remove(action);
        }
        #endregion

        private void OnDestroy()
        {
            _instance = null;
        }

        # region 数据类型：延迟一段时间后调用某方法。允许立刻调用->Invoke、取消调用->Cancel（在初始化后会立刻等待一段时间后调用，需在调用前取消才有效）
        public abstract class DelayInvokeAction
        {
            public Action Action { get; private set; }
            public DelayInvokeAction(Action action)
            {
                Action = action;
            }
            public void Invoke()
            {
                Action?.Invoke();
            }
            public void Cancel()
            {
                Instance.Cancel(this);
            }
        }

        public class DelayInvokeActionScaledTime : DelayInvokeAction
        {
            public float InvokeTime { get; private set; }
            public DelayInvokeActionScaledTime(float delayTime, Action action) : base(action)
            {
                InvokeTime = Time.time + delayTime;
            }
        }

        public class DelayInvokeActionUnscaledTime : DelayInvokeAction
        {
            public float DelayTime { get; private set; }

            public DelayInvokeActionUnscaledTime(float delayTime, Action action) : base(action)
            {
                DelayTime = delayTime;
            }
        }
        #endregion
    }
}