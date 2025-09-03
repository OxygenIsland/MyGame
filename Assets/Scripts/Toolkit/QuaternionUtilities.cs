using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class QuaternionUtilities
    {
        // ?Z?
        public static Quaternion IgnoreZRotation(Quaternion original)
        {
            // ????
            Vector3 euler = original.eulerAngles;

            // Z?
            euler.z = 0;

            // ?????
            Quaternion result = Quaternion.Euler(euler);

            return result;
        }
    }
}
