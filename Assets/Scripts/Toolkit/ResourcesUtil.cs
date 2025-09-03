using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace StarWorld.Common.Utility
{
    public static class ResourcesUtil
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T Load<T>(string path)
            where T : Object
        {
            T res = Resources.Load<T>(path);
            return res;
        }

        public static IEnumerator LoadAsync<T>(string path, UnityAction<T> finishCallBack)
            where T : Object
        {
            ResourceRequest resourceRequest = Resources.LoadAsync<T>(path);
            yield return resourceRequest;
            if (finishCallBack != null)
            {
                finishCallBack.Invoke(resourceRequest.asset as T);
            }
        }
    }
}
