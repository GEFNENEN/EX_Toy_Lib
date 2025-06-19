using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace PoofLibraryManager.Editor
{
    public class PoofLibraryManagerWindow : OdinMenuEditorWindow
    {
        private const string MenuPath = "Assets/_PoofLibrary/menu.json";

        [UnityEditor.MenuItem("Tools/Poof Library Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<PoofLibraryManagerWindow>();
            window.titleContent = new GUIContent("资源目录管理器");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                { "首页", this, EditorIcons.House },
                { "状态", new StatusInfo(), EditorIcons.Info }
            };

            tree.DefaultMenuStyle.Height = 30;
            tree.Config.DrawSearchToolbar = true;

            // 尝试加载目录配置文件
            MenuConfig config = LoadMenuConfig();

            if (config != null && config.menuItems != null && config.menuItems.Count > 0)
            {
                // 添加配置查看器
                tree.Add("配置信息", config, EditorIcons.SettingsCog);
                
                // 添加配置的菜单项
                foreach (var item in config.menuItems)
                {
                    // 支持层级菜单："类别/子类别/资源名称"
                    tree.Add(item.menuPath, LoadAsset(item.assetPath));
                }
            }
            else
            {
                tree.Add("警告", "暂无目录配置，请创建配置文件", EditorIcons.UnityWarningIcon);
                tree.Add("帮助", "在Assets/_PoofLibrary路径下创建menu.json文件", EditorIcons.Info);
            }

            return tree;
        }

        private MenuConfig LoadMenuConfig()
        {
            string fullPath = Path.Combine(Application.dataPath, "../", MenuPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"找不到目录配置文件: {fullPath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                return JsonUtility.FromJson<MenuConfig>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析配置文件失败: {e.Message}");
                return null;
            }
        }

        private Object LoadAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // 尝试加载资源
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            if (asset == null)
            {
                Debug.LogWarning($"无法加载资源: {path}");
                return null;
            }

            return asset;
        }

        // 菜单配置类
        [System.Serializable]
        public class MenuConfig
        {
            public List<MenuItem> menuItems = new List<MenuItem>();
        }

        [System.Serializable]
        public class MenuItem
        {
            public string menuPath;
            public string assetPath;
            [TextArea] public string description;
        }

        // 状态信息类
        public class StatusInfo
        {
            [ShowInInspector, DisplayAsString]
            public string Message => File.Exists(Path.Combine(Application.dataPath, "../", MenuPath))
                ? "目录配置加载成功"
                : "未找到配置文件";

            [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
            public void CreateConfigTemplate()
            {
                string directory = Path.Combine(Application.dataPath, "_PoofLibrary");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }

                string fullPath = Path.Combine(directory, "menu.json");

                if (!File.Exists(fullPath))
                {
                    MenuConfig config = new MenuConfig
                    {
                        menuItems = new List<MenuItem>
                        {
                            new MenuItem
                            {
                                menuPath = "示例/角色模型",
                                assetPath = "Assets/Characters/Hero.prefab",
                                description = "主角角色模型"
                            },
                            new MenuItem
                            {
                                menuPath = "示例/武器资源",
                                assetPath = "Assets/Weapons/Sword.prefab",
                                description = "基础武器预制体"
                            }
                        }
                    };

                    File.WriteAllText(fullPath, JsonUtility.ToJson(config, true));
                    AssetDatabase.Refresh();
                    Debug.Log($"已创建示例配置: {fullPath}");
                }
                else
                {
                    Debug.LogWarning("配置文件已存在");
                }
            }
        }

        // 窗口首页内容
        [BoxGroup("欢迎使用资源目录管理器")]
        [HideLabel]
        [DisplayAsString,TextArea(1,20)]
        public string WelcomeMessage = "此工具用于管理项目资源目录\n"
                                       + "1. 创建Assets/_PoofLibrary/menu.json文件\n"
                                       + "2. 按照JSON格式配置资源目录\n"
                                       + "3. 左侧菜单将自动显示配置的资源目录";

        [BoxGroup("欢迎使用资源目录管理器")]
        [Button("打开配置目录", ButtonSizes.Large)]
        [PropertyOrder(-1)]
        public void OpenConfigFolder()
        {
            string directory = Path.Combine(Application.dataPath, "_PoofLibrary");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            EditorUtility.RevealInFinder(directory);
        }
    }
}