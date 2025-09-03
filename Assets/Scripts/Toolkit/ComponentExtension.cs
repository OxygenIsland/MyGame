using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace StarWorld.Common
{
    public static class ComponentExtension
    {
        public static void RegistEvent(GameObject go, EventTriggerType eType, UnityAction<BaseEventData> action)
        {
            EventTrigger eventTrigger = go.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = go.AddComponent<EventTrigger>();
            }
            EventTrigger.Entry eventEntry = new EventTrigger.Entry()
            {
                eventID = eType
            };

            eventEntry.callback.AddListener(action);
            eventTrigger.triggers.Add(eventEntry);
        }

        public static Bounds CalculateBounds(Transform tran)
        {
            Vector3 resCenter = Vector3.zero;

            List<Renderer> renderers = new List<Renderer>();
            if (tran.GetComponent<Renderer>())
            {
                renderers.Add(tran.GetComponent<Renderer>());
            }
            renderers.AddRange(tran.GetComponentsInChildren<Renderer>());
            if (renderers != null && renderers.Count > 0)
            {
                foreach (var item in renderers)
                {
                    resCenter += item.bounds.center;
                }
                resCenter /= renderers.Count;
            }
            Bounds bounds = new Bounds(resCenter, Vector3.zero);
            foreach (Renderer child in renderers)
            {
                bounds.Encapsulate(child.bounds);
            }
            return bounds;
        }

        public static Bounds CalculateBounds(IEnumerable<GameObject> _trans)
        {
            Vector3 resCenter = Vector3.zero;

            List<Renderer> renderers = new List<Renderer>();
            foreach (var item in _trans)
            {
                if (item.GetComponent<Renderer>())
                {
                    renderers.Add(item.GetComponent<Renderer>());
                }
                renderers.AddRange(item.gameObject.GetComponentsInChildren<Renderer>());
            }
            if (renderers != null && renderers.Count > 0)
            {
                foreach (var item in renderers)
                {
                    resCenter += item.bounds.center;
                }
                resCenter /= renderers.Count;
            }
            Bounds bounds = new Bounds(resCenter, Vector3.zero);
            foreach (Renderer child in renderers)
            {
                bounds.Encapsulate(child.bounds);
            }
            return bounds;
        }

        public static Bounds CalculateBounds(IEnumerable<Renderer> renderers)
        {
            if (renderers == null)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Vector3 resCenter = Vector3.zero;
            if (renderers.Count() > 0)
            {
                foreach (var item in renderers)
                {
                    resCenter += item.bounds.center;
                }
                resCenter /= renderers.Count();
            }
            Bounds bounds = new Bounds(resCenter, Vector3.zero);
            foreach (Renderer child in renderers)
            {
                bounds.Encapsulate(child.bounds);
            }
            return bounds;
        }
    }
}