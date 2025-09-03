using UnityEngine;

namespace DesignPattern.SingleTon
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        protected bool dontDestroyOnLoad = false;
        private static T instance;
        private static object _lock = new object();
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        instance = (T)FindObjectOfType(typeof(T), true);
                        if (FindObjectsOfType(typeof(T), true).Length > 2)
                        {
                            Stark.Core.Logs.Log.Error("there are two instance in the scene!");
                        }
                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();
                        }
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                if (instance != this)
                {
                    Destroy(this);
                    Stark.Core.Logs.Log.Error("there are two instance in the scene! name:" + gameObject.name);
                    return;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }

    /// <summary>
    /// 此单例不会主动创建GameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingletonWithouNew<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        protected bool dontDestroyOnLoad = false;
        private static T instance;
        private static object _lock = new object();
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        instance = (T)FindObjectOfType(typeof(T), true);
                        if (FindObjectsOfType(typeof(T), true).Length > 2)
                        {
                            Stark.Core.Logs.Log.Error("there are two instance in the scene!");
                        }
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                if (instance != this)
                {
                    Destroy(this);
                    Stark.Core.Logs.Log.Error("there are two instance in the scene! name:" + gameObject.name);
                    return;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
