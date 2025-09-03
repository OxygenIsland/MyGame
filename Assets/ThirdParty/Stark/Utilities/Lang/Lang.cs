﻿using Stark.Core.Logs;
using System.Collections.Generic;

namespace Stark.Core.Utilities
{
    /// <summary>
    /// 语言包翻译模块(可加载多个文件)
    /// </summary>
    public class Lang
    {
        private static Dictionary<string, string> s_dict = new Dictionary<string, string>(128);

        /// <summary>
        /// 从本地文件中解析，必须为UTF8编码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        //public static void LoadFromFile<T>(string path) where T : ILanguage, new()
        //{
        //    if (!File.Exists(path))
        //        throw new FileNotFoundException();
        //    path = ExpandFunction.HandlePathTraversalVulnerability(path);
        //    var content = File.ReadAllText(path, Encoding.UTF8);
        //    LoadFromContent<T>(content);
        //}

        /// <summary>
        /// 从string字符串中解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        public static void LoadFromContent<T>(string content) where T : ILanguage, new()
        {
            var language = new T();
            var dict = language.ParseFromContent(content);
            if (dict == null)
                return;

            //进行key冲突检查
            foreach (var key in dict.Keys)
            {
                if (s_dict.ContainsKey(key))
                {
                    Log.Warn($"Already contains key: {key}");
                }
                s_dict[key] = dict[key];
            }

            Log.Info("Load lang success. count={0}", dict.Count);
        }

        /// <summary>
        /// 清空语言包数据
        /// </summary>
        public static void Clear()
        {
            s_dict.Clear();
        }

        /// <summary>
        /// 语言包转换
        /// </summary>
        /// <param name="key"></param>
        public static string Trans(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            string ret;
            if (s_dict.TryGetValue(key, out ret))
                return ret.Replace("\\n", "\n");

            return "※" + key;
            //return source;
        }

        /// <summary>
        /// 语言包转换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        public static string Trans(string key, params object[] args)
        {
            return string.Format(Trans(key), args);
        }
    }
}
