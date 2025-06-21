using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PoofLibraryManager.Editor
{
    public class PoofLibraryManagerWindow : OdinMenuEditorWindow
    {
        [MenuItem("EX_Tools/Poof Library Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<PoofLibraryManagerWindow>();
            window.titleContent = new GUIContent(PoofLibraryConstParam.POOF_LIB_MGR);
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false)
            {
                { "首页", new PoofLibraryHostPage(), EditorIcons.House },
                { "设置", PoofLibrarySetting.Instance, EditorIcons.SettingsCog },
                { "库插件总览", new PoofLibOverview(), EditorIcons.UnityInfoIcon }
            };

            tree.DefaultMenuStyle.Height = 30;
            tree.Config.DrawSearchToolbar = true;

            // 尝试加载目录配置文件
            var config = LoadMenuConfigs();
            
            if (!(config == null || config.Count == 0))
            {
                // 如果有配置文件，添加到菜单树中
                foreach (var item in config)
                {
                    // 添加配置查看器
                    tree.Add(item.Name, item, EditorIcons.List);
                    
                    foreach (var plugin in item.Plugins)
                        tree.Add(plugin.MenuPath, plugin);
                }
            }

            return tree;
        }

        private List<PLMenuConfig> LoadMenuConfigs()
        {
            var repoInfos = PoofLibrarySetting.Instance.repoInfos;
            if (repoInfos == null || repoInfos.Count == 0)
            {
                Debug.LogWarning("没有配置任何仓库信息");
                return null;
            }

            var configs = new List<PLMenuConfig>();

            foreach (var repo in repoInfos)
            {
                var fullPath = Path.Combine(Application.dataPath, "../", repo.localMenuPath);
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"找不到目录配置文件: {fullPath}");
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(fullPath);
                    var config = JsonUtility.FromJson<PLMenuConfig>(json);
                    if (config != null) configs.Add(config);
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析配置文件失败: {e.Message}");
                }
            }

            if (configs.Count != 0) return configs;
            Debug.LogWarning("没有有效的目录配置文件");
            return null;
        }

        private Object LoadAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null) return asset;
            Debug.LogWarning($"无法加载资源: {path}");
            return null;
        }
    }
}