using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class ResourcesManager
    {
        static Dictionary<string, GameObject> resDic = new Dictionary<string, GameObject>();

        public static GameObject Load(string path)
        {
            if (resDic.ContainsKey(path))
            {
                return resDic[path];
            }

            GameObject go = Resources.Load(path) as GameObject;
            resDic[path] = go;

            return go;
        }


        static Hashtable resTable = new Hashtable();

        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (resTable.ContainsKey(path))
            {
                return resTable[path] as T;
            }

            T t = Resources.Load<T>(path);
            resTable[path] = t;

            return t;
        }
    }
}
