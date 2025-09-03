using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarWorld.Common.Utility
{
    public static class MathsTool
    {
        /// <summary>
        /// 计算点到线段的水平面最短距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static float GetPointToPlaneDistance(Vector3 point, Vector3 start, Vector3 end)
        {
            point = new Vector3(point.x, 0, point.z);
            start = new Vector3(start.x, 0, start.z);
            end = new Vector3(end.x, 0, end.z);

            Vector3 sp = point - start;
            Vector3 ep = point - end;
            Vector3 se = end - start;
            Vector3 project = Vector3.Project(sp, se);
            Vector3 line = sp - project;

            if (Vector3.Dot(sp, se) >= 0 && Vector3.Dot(ep, se) <= 0)//判定点是否映射在线段范围内
            {
                return line.magnitude;
            }
            return Mathf.Min(sp.magnitude, ep.magnitude);
        }

    }
}