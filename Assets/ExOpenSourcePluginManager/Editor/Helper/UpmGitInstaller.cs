#if UNITY_EDITOR
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace ExOpenSource.Editor
{
    public static class UpmGitInstaller
    {
        /// <summary>
        ///     package是否已经添加到manifest.json中
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool IsInstalled(string packageName)
        {
            var manifestPath = Path.GetFullPath("Packages/manifest.json");
            // 读取并解析manifest.json
            var manifest = JObject.Parse(File.ReadAllText(manifestPath));
            var dependencies = (JObject)manifest["dependencies"] ?? new JObject();
            return dependencies.ContainsKey(packageName);
        }

        /// <summary>
        ///     添加Git依赖包到项目manifest.json
        /// </summary>
        /// <param name="packageName">包名（如：com.unity.xr.hands）</param>
        /// <param name="gitUrl">Git仓库URL（支持#版本号、?path路径参数）</param>
        /// <param name="allowOverride">是否允许覆盖现有包（默认false）</param>
        public static void AddGitPackage(string packageName, string gitUrl, bool allowOverride = false)
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            // 1. 读取manifest.json
            var manifest = JObject.Parse(File.ReadAllText(manifestPath));
            var dependencies = (JObject)manifest["dependencies"] ?? new JObject();

            // 2. 检查包是否已存在
            if (dependencies.ContainsKey(packageName))
            {
                if (!allowOverride)
                {
                    Debug.LogWarning($"包 {packageName} 已存在！取消操作");
                    return;
                }

                dependencies[packageName] = gitUrl; // 覆盖现有包
            }
            else
            {
                dependencies.Add(packageName, gitUrl); // 添加新包
            }

            // 3. 写入JSON并刷新Unity
            manifest["dependencies"] = dependencies;
            File.WriteAllText(manifestPath, manifest.ToString(Formatting.Indented));
            AssetDatabase.Refresh(); // 触发包管理器重新加载
            Debug.Log($"成功添加: {packageName}\nURL: {gitUrl}");
        }
    }
}
#endif