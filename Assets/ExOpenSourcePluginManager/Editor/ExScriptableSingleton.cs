﻿#if UNITY_EDITOR
namespace ExOpenSource.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditorInternal;
    using UnityEngine;
    
    public class ExScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T s_Instance;
        public static T Instance
        {
            get
            {
                if (!s_Instance)
                {
                    LoadOrCreate();
                }
                return s_Instance;
            }
        }
        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                s_Instance = arr.Length > 0 ? arr[0] as T : s_Instance??CreateInstance<T>();
            }
            else
            {
                Debug.LogError($"save location of {nameof(ExScriptableSingleton<T>)} is invalid");
            }
            return s_Instance;
        }

        public static void Save(bool saveAsText = true)
        {
            if (!s_Instance) return;

            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                UnityEngine.Object[] obj = new T[1] { s_Instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
                //Debug.Log($"Saved ScriptableSingleton to {filePath}");
            }
        }

        public static void UpdateAsset(T asset)
        {
            if (asset == null) return;
            s_Instance = asset;
        }
        
        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Where(v => v is PLFilePathAttribute)
                .Cast<PLFilePathAttribute>()
                .FirstOrDefault()
                ?.filepath;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PLFilePathAttribute : Attribute
    {
        internal string filepath;
        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public PLFilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }
            filepath = path;
        }
    }
}
#endif