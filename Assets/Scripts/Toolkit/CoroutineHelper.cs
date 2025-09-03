using System.Collections;
using System.Collections.Generic;
using StarWorld.FrameWork;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    internal class SingleCoroutine
    {
        private IEnumerator coroutine = null;
        private GameObject mGameObject = null;
        private MonoBehaviour behaviour = null;
        private System.Action<SingleCoroutine> RemoveCallback = null;
        private bool mINFINITE = false;

        internal SingleCoroutine(IEnumerator coroutine, GameObject mGameObject, System.Action<SingleCoroutine> AddCallback, System.Action<SingleCoroutine> RemoveCallback, bool mINFINITE)
        {
            this.mGameObject = mGameObject;
            this.coroutine = coroutine;
            this.RemoveCallback = RemoveCallback;
            this.mINFINITE = mINFINITE;

            if (null != AddCallback)
                AddCallback(this);

            GameObject.DontDestroyOnLoad(gameObject);

            behaviour = mGameObject.GetComponent<MyCoroutine>();
            if (null != behaviour)
                behaviour.StartCoroutine(ExecuteCoroutine());
        }

        public void Remove()
        {
            if (null != behaviour)
                behaviour.StopAllCoroutines();

            if (null != mGameObject)
            {
                GameObject.Destroy(mGameObject);
                mGameObject = null;
            }
        }

        IEnumerator ExecuteCoroutine()
        {
            yield return coroutine;
            if (null != RemoveCallback)
            {
                RemoveCallback(this);
            }
        }

        public bool INFINITE
        {
            get
            {
                return mINFINITE;
            }
        }


        public GameObject gameObject
        {
            get
            {
                return mGameObject;
            }
        }


        public IEnumerator Coroutine
        {
            get
            {
                return coroutine;
            }
        }
    }

    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        private List<CoroutineWrapper> mlist = null;
        private List<SingleCoroutine> mSingleCoroutineList = null;
        private MonoBehaviour behaviour = null;
        private static object lock_obj = new object();

        private class CoroutineWrapper
        {
            public bool INFINITE = false;
            public IEnumerator Coroutine = null;

            public CoroutineWrapper(IEnumerator Coroutine, bool INFINITE)
            {
                this.INFINITE = INFINITE;
                this.Coroutine = Coroutine;
            }
        }

        public CoroutineHelper()
        {
            mlist = new List<CoroutineWrapper>();
            mSingleCoroutineList = new List<SingleCoroutine>();
            GameObject gameObject = new GameObject("FastCoroutine", typeof(MyCoroutine));
            if (null != gameObject)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(gameObject);
                behaviour = gameObject.GetComponent<MonoBehaviour>();
                behaviour.StartCoroutine(MakeCoroutine());
            }
        }

        //添加携程
        public void AddCoroutine(IEnumerator coroutine, bool INFINITE = false)
        {
            CoroutineWrapper wrapper = new CoroutineWrapper(coroutine, INFINITE);
            if ((null != wrapper) && (null != mlist)) mlist.Add(wrapper);
        }

        public Coroutine Sequence(List<IEnumerator> coroutines)
        {
            GameObject gameObject = new GameObject("SequenceCoroutine", typeof(MyCoroutine));
            GameObject.DontDestroyOnLoad(gameObject);
            var behaviour = gameObject.GetComponent<MyCoroutine>();
            return behaviour.RunSequence(coroutines);
        }

        public void Parallel(List<IEnumerator> coroutines)
        {
            MyCoroutine behaviour = null;
            var go = GameObject.Find("ParallelCoroutine");
            if (GameObject.Find("ParallelCoroutine") == null)
            {
                go = new GameObject("ParallelCoroutine", typeof(MyCoroutine));
                GameObject.DontDestroyOnLoad(go);
                behaviour = go.GetComponent<MyCoroutine>();
            }
            else
            {
                behaviour = go.GetComponent<MyCoroutine>();
            }

            behaviour.RunParallel(coroutines);
        }

        public void AddCoroutineThread(IEnumerator coroutine, bool INFINITE = false)
        {
            lock (lock_obj)
            {
                CoroutineWrapper wrapper = new CoroutineWrapper(coroutine, INFINITE);
                if ((null != wrapper) && (null != mlist)) mlist.Add(wrapper);
            }
        }

        //回调移除携程
        private void RemoveCoroutine(SingleCoroutine coroutine)
        {
            lock (lock_obj)
            {
                if (null != mlist)
                {
                    for (int i = 0; i < mlist.Count; i++)
                    {
                        CoroutineWrapper wrapper = mlist[i];
                        if (wrapper.Coroutine == coroutine.Coroutine)
                        {
                            mlist.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (null != coroutine)
            {
                coroutine.Remove();
                mSingleCoroutineList.Remove(coroutine);
            }
        }

        //移除携程
        public void RemoveCoroutine(IEnumerator coroutine)
        {
            lock (lock_obj)
            {
                if (null != mlist)
                {
                    for (int i = 0; i < mlist.Count; i++)
                    {
                        CoroutineWrapper wrapper = mlist[i];
                        if (wrapper.Coroutine == coroutine)
                        {
                            mlist.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            SingleCoroutine result = null;
            if (null != mSingleCoroutineList)
            {
                for (int i = 0; i < mSingleCoroutineList.Count; i++)
                {
                    SingleCoroutine singleCoroutine = mSingleCoroutineList[i];
                    if ((null != singleCoroutine) && (singleCoroutine.Coroutine.ToString().Equals(coroutine.ToString())))
                    {
                        result = singleCoroutine;
                        break;
                    }
                }

                if (null != result)
                {
                    //Debug.LogError("移除 coroutine = " + result.Coroutine);
                    result.Remove();
                    mSingleCoroutineList.Remove(result);
                }
            }
        }

        public void Destroy()
        {
            if (null != mlist)
                mlist.Clear();

            if (null != mSingleCoroutineList)
            {
                List<SingleCoroutine> removeList = new List<SingleCoroutine>();
                if (null != removeList)
                {
                    for (int i = 0; i < mSingleCoroutineList.Count; i++)
                    {
                        SingleCoroutine singleCoroutine = mSingleCoroutineList[i];
                        if ((null != singleCoroutine) && !singleCoroutine.INFINITE)
                            removeList.Add(singleCoroutine);
                    }

                    while (removeList.Count > 0)
                    {
                        SingleCoroutine coroutine = removeList[0];
                        removeList.RemoveAt(0);

                        if (null != coroutine)
                        {
                            coroutine.Remove();
                            mSingleCoroutineList.Remove(coroutine);
                        }
                    }
                    removeList = null;
                }
            }
        }

        private IEnumerator MakeCoroutine()
        {
            while (null != mlist)
            {
                while (mlist.Count > 0)
                {
                    CoroutineWrapper coroutine = null;
                    lock (lock_obj)
                    {
                        coroutine = mlist[0];
                        mlist.RemoveAt(0);
                    }

                    IEnumerator _coroutine = coroutine.Coroutine;
                    bool _INFINITE = coroutine.INFINITE;


                    if (null != mSingleCoroutineList)
                    {
                        GameObject gameObject = new GameObject("SingleCoroutine", typeof(MyCoroutine));
                        if (null != gameObject)
                        {
                            gameObject.hideFlags = HideFlags.HideInHierarchy;

                            SingleCoroutine singleCoroutine = new SingleCoroutine(_coroutine, gameObject, (x) => { mSingleCoroutineList.Add(x); }, RemoveCoroutine, _INFINITE);
                        }
                    }
                    yield return null;
                };
                yield return null;
            };
        }
    }
}
