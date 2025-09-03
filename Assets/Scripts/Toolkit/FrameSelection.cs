using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class FrameSelection
    {
        static Bounds CalculateBounds(GameObject go)
        {
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            var rList = go.GetComponentsInChildren<Renderer>();
            foreach (var r in rList)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        public static void FocusCameraOnGameObject(Camera c, GameObject root)
        {
            Bounds b = CalculateBounds(root);

            Vector3 max = b.size;
            // Get the radius of a sphere circumscribing the bounds
            float radius = max.magnitude / 2f;

            var fov = GetMiniFov(c);

            var dist = GetDist(radius, fov);

            //Get the position of the point which is in the forward direction of the camera


            Vector3 posForward = c.transform.position + c.transform.forward * dist;
            //Get the vector between posForward and bound box center
            Vector3 move = posForward - b.center;
            //apply move to root position
            Vector3 rootAfterMove = root.transform.position + move;
            root.transform.position = rootAfterMove;
        }

        public static float GetFocusCameraDistance(Camera c, GameObject root)
        {
            Bounds b = CalculateBounds(root);

            Vector3 max = b.size;
            // Get the radius of a sphere circumscribing the bounds
            float radius = max.magnitude / 2f;

            var fov = GetMiniFov(c);
            var dist = GetDist(radius, fov);
            return dist;
        }

        private static float GetDist(float radius, float fov)
        {
            float dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));
            if (dist < 1)
            {
                dist = 1;
            }
            return dist;
        }

        private static float GetMiniFov(Camera c)
        {
            // Get the horizontal FOV, since it may be the limitation of the two FOVs to properly encapsulate the objects
            float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(c.fieldOfView * Mathf.Deg2Rad / 2f) * c.aspect) * Mathf.Rad2Deg;
            // Use the smaller FOV as it limits what would get cut off by the frustum        
            float fov = Mathf.Min(c.fieldOfView, horizontalFOV);

            return fov;
        }
    }
}