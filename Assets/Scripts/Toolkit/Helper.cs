using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class Helper
    {
        public static Vector3 getVector3FromString(string rString)
        {
            try
            {
                string[] array = rString.Substring(1, rString.Length - 2).Split(',');
                float x = float.Parse(array[0]);
                float y = float.Parse(array[1]);
                float z = float.Parse(array[2]);
                return new Vector3(x, y, z);
            }
            catch
            {
                return Vector3.zero;
            }
        }

        public static string transferVector3ToString(Vector3 v)
        {
            return "(" + v.x + "," + v.y + "," + v.z + ")";
        }

        public static Quaternion getQuaternionFromString(string rString)
        {
            string[] array = rString.Substring(1, rString.Length - 2).Split(',');
            float x = float.Parse(array[0]);
            float y = float.Parse(array[1]);
            float z = float.Parse(array[2]);
            float w = float.Parse(array[3]);
            return new Quaternion(x, y, z, w);
        }

        public static Vector4 getVector4FromString(string rString)
        {
            try
            {
                string[] array = rString.Substring(1, rString.Length - 2).Split(',');
                float x = float.Parse(array[0]);
                float y = float.Parse(array[1]);
                float z = float.Parse(array[2]);
                float w = float.Parse(array[3]);
                return new Vector4(x, y, z, w);
            }
            catch
            {
                return Vector4.zero;
            }
        }

        public static string transferQuaternionToString(Quaternion q)
        {
            return "(" + q.x + "," + q.y + "," + q.z + "," + q.w + ")";
        }

        public static string transferVector4ToString(Vector4 q)
        {
            return "(" + q.x + "," + q.y + "," + q.z + "," + q.w + ")";
        }
    }
}