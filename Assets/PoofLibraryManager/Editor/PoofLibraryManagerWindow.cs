using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PoofLibraryManager.Editor
{
    public class PoofLibraryManagerWindow : OdinMenuEditorWindow
    {

        [UnityEditor.MenuItem("EX_Tools/Poof Library Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<PoofLibraryManagerWindow>();
            window.titleContent = new GUIContent(PoofLibraryConstParam.POOF_LIB_MGR);
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                { "首页", new PoofLibraryHostPage(), EditorIcons.House },
                { "状态", new PoofLibrarySettingPage(), EditorIcons.Info }
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
            string fullPath = Path.Combine(Application.dataPath, "../", PoofLibraryConstParam.MenuPath);

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
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

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

        // 实例引用用于下载状态更新
    private static PoofLibraryManagerWindow instance;
    public static PoofLibraryManagerWindow Instance => instance;
    
    private bool isDownloading;
    private UnityWebRequest downloadRequest;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        instance = this;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (instance == this) instance = null;
        
        // 确保取消下载请求
        if (isDownloading && downloadRequest != null)
        {
            downloadRequest.Abort();
        }
    }
    
    public void StartDownload(string url)
    {
        if (isDownloading) return;
        
        isDownloading = true;
        EditorCoroutineUtility.StartCoroutineOwnerless(DownloadMenuConfig(url));
    }
    
    private IEnumerator DownloadMenuConfig(string url)
    {
        // 确保目录存在
        string directory = Path.Combine(Application.dataPath, "_PoofLibrary");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string downloadStatus = "开始下载配置文件...";
        float progress = 0f;
        
        using (downloadRequest = UnityWebRequest.Get(url))
        {
            // 添加下载进度回调
            downloadRequest.SendWebRequest();
            
            while (!downloadRequest.isDone)
            {
                progress = downloadRequest.downloadProgress;
                downloadStatus = $"下载中... {Mathf.RoundToInt(progress * 100)}%";
                yield return null;
            }
            
            // 下载完成
            if (downloadRequest.result == UnityWebRequest.Result.Success)
            {
                string savePath = Path.Combine(directory, "menu.json");
                File.WriteAllText(savePath, downloadRequest.downloadHandler.text);
                
                downloadStatus = "配置文件下载成功！";
                progress = 1f;
                
                // 短暂显示完成状态
                yield return new WaitForSeconds(1f);
                
                // 刷新资源数据库
                AssetDatabase.Refresh();
                
                // 重新加载菜单
                ForceMenuTreeRebuild();
            }
            else
            {
                downloadStatus = $"下载失败: {downloadRequest.error}";
            }
        }
        
        isDownloading = false;
        downloadRequest = null;
    }
    }
}