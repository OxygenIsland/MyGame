using System;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class SolutionTool
    {
        /// <summary>
        /// 处理一个省略号
        /// </summary>
        /// <param name="value"></param>
        /// <param name="limit"></param>
        /// <param name="ellipsis">省略符号，如...</param>
        /// <returns></returns>
        public static string EllipsisString(string value, int limit, string ellipsis)
        {
            string outputStr = value;
            outputStr = LimitStringByUTF8(value, limit) + (CheckStringByUTF8(value, limit + 1) ? ellipsis : "");
            return outputStr;
        }
        public static bool CheckStringByUTF8(string temp, int limit)
        {
            if (string.IsNullOrEmpty(temp))
            {
                return false;
            }
            bool overflow = false;
            int count = 0;

            for (int i = 0; i < temp.Length; i++)
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
        public static string LimitStringByUTF8(string temp, int limit)
        {
            if (string.IsNullOrEmpty(temp))
            {
                return "";
            }
            string outputStr = "";
            int count = 0;

            for (int i = 0; i < temp.Length; i++)
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

        public static string GetDate(long time)//时间戳转换为时间
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime dt = startTime.AddMilliseconds(time);
            //string t = dt.ToString("yyyy/MM/dd HH:mm:ss");
            string t = dt.ToString("yyyy/MM/dd HH:mm");
            return t;
        }

        public static long GetTimeTicks(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static Vector3 GetDistancePos(Vector3 pos, Vector3 targetPos, float distance)
        {
            Vector3 directionToTarget = targetPos - pos;
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return pos;
            }
            return targetPos - directionToTarget.normalized * distance;
        }

        public static void SetBillBoardRotation(Transform trans, Transform target)
        {
            Vector3 directionToTarget = target.position - trans.position;
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }
            trans.rotation = Quaternion.LookRotation(-directionToTarget);
        }
    }
}