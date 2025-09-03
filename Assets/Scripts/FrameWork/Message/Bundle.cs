/**
 * ==========================================
 * Author：xuzq9
 * CreatTime：2023.7.5
 * Description：A common data container
 * ==========================================
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace StarWorld
{
    /// <summary>
    /// 一个存放任意类型数据的容器
    /// </summary>
    public class Bundle
    {
        private Dictionary<string, BundleData> mdataDict = new Dictionary<string, BundleData>();

        public Dictionary<string,BundleData> DataDict
        {
            get
            {
                return mdataDict;
            }
        }

        [Serializable]
        public class BundleData
        {
            public Type type;
            public System.Object obj;

            public BundleData(Type ty, System.Object ob)
            {
                type = ty;
                obj = ob;
            }
        }

        /// <summary>
        /// 设置bundle的数据
        /// 若key相同，后一个会覆盖上一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public Bundle SetValue<T>(string key, T val)
        {
            if (val == null)
            {
                return this;
            }
            if (mdataDict.ContainsKey(key))
            {
                Debug.Log("the key :" + key + " is already exist!");
                mdataDict.Remove(key);
            }
            BundleData data;
            try
            {
                data = new BundleData(val.GetType(), val);
            }
            catch (System.Exception e)
            {
                if (e != null)
                {
                    Debug.LogWarning("[Bundle] SetValue val.GetType() error" + e.Message);
                }
            }
            finally
            {
                data = new BundleData(default, val);
            }
            mdataDict.Add(key, data);

            return this;
        }

        /// <summary>
        /// 获取bundle中的数据
        /// 在此之前应该先用Contains方法检测是否存在此数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            T val = default(T);
            BundleData data;
            if (mdataDict.TryGetValue(key, out data) && (data.obj is T))
            {
                val = (T)data.obj;
            }
            else
            {
                Debug.Log("can not get the key:" + key);
            }

            return val;
        }

        /// <summary>
        /// 检测是否存在此数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains<T>(string key)
        {
            BundleData data;
            //&& (data.type is T)
            //T t = default(T);
            if (mdataDict.TryGetValue(key, out data) && (data.obj is T))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder st = new StringBuilder();
            foreach (var item in mdataDict)
            {
                st.Append($"[{item.Key},{item.Value.obj}] ");
            }
            return st.ToString();
        }

        public string ToJsonString()
        {
            JObject mJObject = new JObject();
            foreach (var item in mdataDict)
            {
                mJObject.Add(new JProperty(item.Key, item.Value.obj));
            }
            return mJObject.ToString();
        }

        public void FromJsonString(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return;
            }
            try
            {
                JObject mJObject = JObject.Parse(jsonString);
                var mJlist = mJObject.GetEnumerator();
                while (mJlist.MoveNext())
                {
                    var kv = mJlist.Current;
                    this.SetValue<string>(kv.Key, kv.Value.ToString());
                }
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    Debug.LogError("[Bundle] FromJsonString error" + e.ToString());
                }
            }
        }

        public bool TryGetValue<T>(string key, out T val)
        {
            val = default(T);
            BundleData data;
            if (!mdataDict.TryGetValue(key, out data))
            {
                Debug.LogWarning("TryGetValue failed");
                return false;
            }
            if (!(data.obj is T))
            {
                Debug.LogWarning("TryGetValue isn't the same type " + data.obj.GetType());
                return false;
            }
            val = (T)data.obj;
            return true;
        }
    }
}