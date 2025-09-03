using System.IO;
using UnityEngine;

namespace StarWorld.Common.Utility
{
    public class FileTool
    {
        /// <summary>
        /// 加载本地图片
        /// </summary>
        public static Sprite LoadImageByLocalPath(string path, int width, int height)
        {
            Sprite sprite;
            //double startTime = (double)Time.time;
            //创建文件读取流
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);

            //释放文件读取流
            fileStream.Close();
            //释放本机屏幕资源
            fileStream.Dispose();
            fileStream = null;

            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(bytes);

            //创建Sprite
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            return sprite;
            //startTime = (double)Time.time - startTime;
            //Debug.Log("IO加载" + startTime);
        }

    }
}