using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class Expand
    {
        /// <summary>
        /// 获取字符串中的字节长度1个汉字=2个字节
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static int GetLengthByGB2312(this string inputString)
        {
            return Encoding.GetEncoding("gb2312").GetByteCount(inputString);
        }

        public static void ClearChildGameobject(this Transform target)
        {
            if (target != null)
            {
                for (int k = 0;k < target.childCount;k++)
                {
                    GameObject.Destroy(target.GetChild(k).gameObject);
                }
            }
        }

        public static void SetChildGameobjectActive(this Transform target, bool active)
        {
            if (target != null)
            {
                for (int k = 0;k < target.childCount;k++)
                {
                    target.GetChild(k).gameObject.SetActive(active);
                }
            }
        }

        public static List<T> Swap<T>(List<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
            return list;
        }

        //限制字符总长度小于对于limit个中文的长度
        //UTF8编码格式（汉字2byte，英文1byte）
        public static string LimitStringByUTF8(string temp, int limit)
        {
            string outputStr = "";
            int count = 0;

            for (int i = 0;i < temp.Length;i++)
            {
                string tempStr = temp.Substring(i, 1);
                int byteCount = System.Text.ASCIIEncoding.UTF8.GetByteCount(tempStr);
                if (byteCount > 1)
                {
                    count += 2;
                }
                else
                {
                    count += 1;
                }
                //限制输入字符长度小于对于20个中文长度
                if (count <= limit)
                {
                    outputStr += tempStr;
                }
                else
                {
                    break;
                }
            }
            return outputStr;
        }

        public static bool CheckStringByUTF8(string temp, int limit)
        {
            bool overflow = false;
            int count = 0;

            for (int i = 0;i < temp.Length;i++)
            {
                string tempStr = temp.Substring(i, 1);
                int byteCount = System.Text.ASCIIEncoding.UTF8.GetByteCount(tempStr);
                if (byteCount > 1)
                {
                    count += 2;
                }
                else
                {
                    count += 1;
                }
                if (count >= limit)
                {
                    overflow = true;
                }
            }
            return overflow;
        }

        /// <summary>
        /// 处理一个省略号
        /// </summary>
        /// <param name="value"></param>
        /// <param name="limit"></param>
        /// <param name="ellipsis">省略符号，如...</param>
        /// <returns></returns>
        public static string EllipsisString(string value,int limit,string ellipsis)
        {
            string outputStr = value;
            outputStr = LimitStringByUTF8(value, limit) + (CheckStringByUTF8(value, limit + 1) ? ellipsis : "");
            return outputStr;
        }


        public static void AddRange<T>(this HashSet<T> hashSet, List<T> range)
        {
            if (hashSet == null)
            {
                hashSet = new HashSet<T>();
            }
            if (range != null && range.Count > 0)
            {
                foreach (T item in range)
                {
                    if (item != null)
                    {
                        hashSet.Add(item);
                    }
                }
            }
        }

        public static T GetComponentExpend<T>(this GameObject transform, bool allowSubclass = false)
        {
            T components = default;
            T temp = transform.GetComponent<T>();
            if (temp != null)
            {
                if (temp.ToString().Equals("null"))
                {
                    temp = default;
                }
            }

            if (temp != null && TypeFilter(temp.GetType(), typeof(T), allowSubclass))
            {
                components = temp;
            }

            return components;
        }

        /// <summary>
        /// 获取所有子物体下的T组件，以防自带的超过3层或者未激活状态查找不到
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static List<T> GetComponentFormChild<T>(this Transform transform, bool allowSubclass = false)
        {
            List<T> components = new List<T>();
            T temp = transform.GetComponent<T>();
            if (temp != null)
            {
                if (temp.ToString().Equals("null"))
                {
                    temp = default;
                }
            }

            if (temp != null && TypeFilter(temp.GetType(), typeof(T), allowSubclass))
            {
                components.Add(temp);
            }

            if (transform.childCount > 0)
            {
                Transform child;
                for (int i = 0;i < transform.childCount;i++)
                {
                    child = transform.GetChild(i);
                    List<T> @var = GetComponentFormChild<T>(child, allowSubclass);
                    if (@var != null && @var.Count > 0)
                    {
                        components.AddRange(@var);
                    }
                }
            }
            return components;
        }

        public static List<T> GetRealComponentsInChildren<T>(this Transform transform, bool allowSubclass = false)
        {
            List<T> list = new List<T>();
            list.AddRange(transform.GetComponentsInChildren<T>(allowSubclass));
            list.RemoveAt(0);
            return list;
        }
        public static T GetRealComponent<T>(this Transform transform, bool allowSubclass = false)
        {
            T component = default;
            T temp = transform.GetComponent<T>();
            if (temp != null)
            {
                if (temp.ToString().Equals("null"))
                {
                    temp = default;
                }
            }

            if (temp != null && TypeFilter(temp.GetType(), typeof(T), allowSubclass))
            {
                component = temp;
            }

            return component;
        }

        private static bool TypeFilter(Type curType, Type target, bool allowSubclass)
        {
            bool equals = false;
            if (allowSubclass)
            {
                equals = curType.IsSubclassOf(target);
            }
            else
            {
                equals = (curType == target);
            }
            return equals;
        }

        /// <summary>
        /// 获取模型包围盒的中心点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Vector3 CENTER(this Transform model)
        {
            Vector3 result = Vector3.zero;
            int counter = 0;
            calculateCenter(model, ref result, ref counter);
            return result / counter;
        }


        /// <summary>
        /// 获取模型包围盒
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Bounds BOUNDS(this Transform model, Transform centerTra)
        {
            Vector3 oldPos = model.position;
            model.position = Vector3.zero;
            Vector3 scalueValue = scaleValue(model);
            Vector3 center = model.CENTER();
            centerTra.position = center;
            Bounds resultBounds = new Bounds(Vector3.zero, Vector3.zero);
            calculateBounds(model, ref resultBounds);
            model.position = oldPos;
            //center.Scale(scalueValue1);
            resultBounds.center = center;
            resultBounds.size = new Vector3(resultBounds.size.x / scalueValue.x, resultBounds.size.y / scalueValue.y, resultBounds.size.z / scalueValue.z);
           
            return resultBounds;
        }

        private static void calculateCenter(Transform model, ref Vector3 result, ref int counter)
        {
            Renderer renderer = model.GetComponent<Renderer>();
            MeshFilter mf = model.GetComponent<MeshFilter>();
            ;
            if (renderer != null)
            {
                result += renderer.bounds.center;
                result += mf.sharedMesh.bounds.center;
                counter++;
            }
           
            if (model.childCount.Equals(0))
            {
                return;
            }
            List<Transform> childModels = model.GetRealComponentsInChildren<Transform>();
            for (int i = 0;i < childModels.Count;i++)
                calculateCenter(childModels[i], ref result, ref counter);
        }

        private static Vector3 scaleValue(Transform model)
        {
            Vector3 result = model.localScale;
            return calculateScale(model, ref result);
        }

        private static Vector3 calculateScale(Transform model, ref Vector3 value)
        {
            if (model.parent)
            {
                Vector3 scale = model.parent.localScale;
                value = new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
                calculateScale(model.parent, ref value);
            }
            return value;
        }

        private static void calculateBounds(Transform model, ref Bounds bounds)
        {
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer != null)
            {
                BoxCollider boxCollider = model.gameObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    model.gameObject.AddComponent<BoxCollider>();
                }
                bounds.Encapsulate(renderer.bounds);
            }

            if (model.childCount.Equals(0))
            {
                return;
            }
           
            List<Transform> childModels = model.GetRealComponentsInChildren<Transform>();
            for (int i = 0;i < childModels.Count;i++)
            {
                calculateBounds(childModels[i], ref bounds);
            }   
        }
    }
}