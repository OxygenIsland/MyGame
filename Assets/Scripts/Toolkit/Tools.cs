using System;
using System.Text;
using UnityEngine;


namespace StarWorld.Common.Utility
{
    public static class Tools
    {
        public static Transform FindChild(Transform trsParent, string childName)
        {
            Transform child = trsParent.Find(childName);

            if (child != null)
            {
                return child;
            }

            Transform go = null;
            for (int i = 0; i < trsParent.childCount; i++)
            {
                child = trsParent.GetChild(i);
                go = FindChild(child, childName);
                if (go != null)
                {
                    return go;
                }
            }

            return null;
        }

        public static string transferVector3ToString(Vector3 v)
        {
            return "(" + v.x + "," + v.y + "," + v.z + ")";
        }

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

        public static Sprite GetSprite(Texture2D texture)
        {
            Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sp;
        }

        public static void SetPositionY(Transform obj, float y)
        {
            obj.position = new Vector3(obj.position.x, y, obj.position.z);
        }

        public static void GetTime()
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string hour = DateTime.Now.Hour < 10 ? "0" + DateTime.Now.Hour.ToString() : DateTime.Now.Hour.ToString();
            string minute = DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();
            string second = DateTime.Now.Second < 10 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(year);
            stringBuilder.Append(month);
            stringBuilder.Append(day);
            stringBuilder.Append("-");
            stringBuilder.Append(hour);
            stringBuilder.Append(":");
            stringBuilder.Append(minute);
            stringBuilder.Append(":");
            stringBuilder.Append(second);

            Debug.Log($"DTViewer:当前时间 = {stringBuilder.ToString()}");
        }

        /// <summary>
        /// 获取时间戳-单位秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampSecond()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                return Convert.ToInt64(ts.TotalSeconds);
            }
            catch (Exception ex)
            {
                Debug.Log($"DTViewer:GetTimeStampSecond Error = {ex}");
                return 0;
            }
        }

        /// <summary>
        /// 获取时间戳-单位毫秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampMilliSecond()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                return Convert.ToInt64(ts.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Debug.Log($"DTViewer:GetTimeStampMilliSecond Error = {ex}");
                return 0;
            }
        }

        /// <summary>
        /// TextureToBase64普通转换
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static string TextureToBase64(Texture2D texture2D)
        {
            byte[] imageData = texture2D.EncodeToJPG();
            Debug.LogFormat("DTViewer : Tools-TextureToBase64-" + imageData.Length);
            string baser64 = Convert.ToBase64String(imageData);
            return baser64;
        }

        /// <summary>
        /// TextureToBase64网页解析专用
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static string TextureToBase64_Prefixing(Texture2D texture2D)
        {
            return "data:image/jpeg;base64," + TextureToBase64(texture2D);
        }
    }
}