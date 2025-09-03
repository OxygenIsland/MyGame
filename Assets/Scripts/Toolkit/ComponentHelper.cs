/*
┌────────────────────────────────────────────────────────┐
│	Author：Jeff
│	Description：
│
│
└────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace StarWorld.Common.Utility
{
	public static class ComponentHelper
	{
        public static T GetComponentFromChild<T>(Transform root, string childName)
        {
            var trans = root.Find(childName);
            if (trans == null)
            {
                Debug.LogError("Can't find btn transform");
                return default;
            }
            return trans.GetComponent<T>();
        }

        public static List<T> GetComponentsFromChild<T>(Transform root, List<T> list)
        {
            T t = root.GetComponent<T>();
            if (t != null)
            {
                list.Add(t);
            }
            for (int i = 0; i < root.childCount; i++)
            {
                GetComponentsFromChild(root.GetChild(i), list);
            }
            return list;
        }

        static public T FindInParents<T>(Transform trans) where T : Component
        {
            T comp = trans.GetComponent<T>();
            if (comp == null)
            {
                Transform t = trans.transform.parent;

                while (t != null && comp == null)
                {
                    comp = t.gameObject.GetComponent<T>();
                    t = t.parent;
                }
            }
            return comp;
        }

        static public void SetTransformLayer(Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            for (int i = 0; i < trans.childCount; i++)
            {
                SetTransformLayer(trans.GetChild(i), layer);
            }
        }
    }
}