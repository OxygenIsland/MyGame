using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public static class TimestampTool
    {
        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetNowTimestamp()//毫秒时间戳
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (System.DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        public static string GetNowDate()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (System.DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 
            DateTime dt = startTime.AddMilliseconds(t);
            string st = dt.ToString("yyyy/MM/dd HH:mm:ss");
            return st;

        }
        public static long ConvertDateTimeToInt(DateTime dateTime)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (dateTime.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        public static string GetDate(long time)//时间戳转换为时间
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime dt = startTime.AddMilliseconds(time);
            //string t = dt.ToString("yyyy/MM/dd HH:mm:ss");
            string t = dt.ToString("yyyy/MM/dd HH:mm");
            return t;
        }
    }
}
