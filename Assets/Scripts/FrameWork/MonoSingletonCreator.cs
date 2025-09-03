using System;
using System.Reflection;
#if UNITY_5_6_OR_NEWER
using UnityEngine;
using Object = UnityEngine.Object;
#endif

namespace StarWorld.FrameWork
{
    public interface ISingleton
    {
        void OnSingletonInit();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MonoSingletonPath : Attribute
    {
        private string mPathInHierarchy;

        public MonoSingletonPath(string pathInHierarchy)
        {
            mPathInHierarchy = pathInHierarchy;
        }

        public string PathInHierarchy
        {
            get { return mPathInHierarchy; }
        }
    }

    public static class MonoSingletonCreator
    {
        public static bool IsUnitTestMode { get; set; }

        public static T CreateMonoSingleton<T>() where T : MonoBehaviour, ISingleton
        {
            T instance = null;

            if (!IsUnitTestMode && !Application.isPlaying) return instance;
            instance = Object.FindObjectOfType<T>();

            if (instance != null)
            {
                instance.OnSingletonInit();
                return instance;
            }

            MemberInfo info = typeof(T);
            var attributes = info.GetCustomAttributes(true);
            foreach (var atribute in attributes)
            {
                var defineAttri = atribute as MonoSingletonPath;
                if (defineAttri == null)
                {
                    continue;
                }

                instance = CreateComponentOnGameObject<T>(defineAttri.PathInHierarchy, true);
                break;
            }

            if (instance == null)
            {
                var obj = new GameObject(typeof(T).Name);
                if (!IsUnitTestMode)
                    Object.DontDestroyOnLoad(obj);
                instance = obj.AddComponent<T>();
            }

            instance.OnSingletonInit();
            return instance;
        }

        private static T CreateComponentOnGameObject<T>(string path, bool dontDestroy) where T : MonoBehaviour
        {
            var obj = FindGameObject(path, true, dontDestroy);
            if (obj == null)
            {
                obj = new GameObject("Singleton of " + typeof(T).Name);
                if (dontDestroy && !IsUnitTestMode)
                {
                    Object.DontDestroyOnLoad(obj);
                }
            }

            return obj.AddComponent<T>();
        }

        private static GameObject FindGameObject(string path, bool build, bool dontDestroy)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var subPath = path.Split('/');
            if (subPath == null || subPath.Length == 0)
            {
                return null;
            }

            return FindGameObject(null, subPath, 0, build, dontDestroy);
        }

        private static GameObject FindGameObject(GameObject root, string[] subPath, int index, bool build,
            bool dontDestroy)
        {
            GameObject client = null;

            if (root == null)
            {
                client = GameObject.Find(subPath[index]);
            }
            else
            {
                var child = root.transform.Find(subPath[index]);
                client = child?.gameObject;
            }

            if (client == null && build)
            {
                client = new GameObject(subPath[index]);
                if (root != null)
                {
                    client.transform.SetParent(root.transform);
                }

                if (dontDestroy && index == 0 && !IsUnitTestMode)
                {
                    GameObject.DontDestroyOnLoad(client);
                }
            }

            if (client == null)
            {
                return null;
            }

            return ++index == subPath.Length ? client : FindGameObject(client, subPath, index, build, dontDestroy);
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                        var parentGo = GameObject.Find("Instance");
                        if (parentGo == null)
                        {
                            parentGo = new GameObject("Instance");
                        }
                        _instance.transform.SetParent(parentGo.transform);
                    }

                    if (_instance is ISingleton)
                    {
                        ((ISingleton)_instance).OnSingletonInit();
                    }
                }

                return _instance;
            }
        }
    }

    public static class MonoSingletonProperty<T> where T : MonoBehaviour
    {
        private static T mInstance;

        public static T Instance
        {
            get
            {
                if (null == mInstance)
                {
                    mInstance = GameObject.FindObjectOfType<T>();
                    if (null == mInstance)
                    {
                        mInstance = MonoSingleton<T>.Instance;
                    }
                }

                return mInstance;
            }
        }

        public static void Release()
        {
            if (MonoSingletonCreator.IsUnitTestMode)
            {
                Object.DestroyImmediate(mInstance.gameObject);
            }
            else
            {
                Object.Destroy(mInstance.gameObject);
            }

            mInstance = null;
        }
    }
}