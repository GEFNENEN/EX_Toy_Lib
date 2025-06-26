using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ExOpenSource.Editor
{
    public class PluginInformationPage
    {
        private ExMenuConfig _menuConfig;
        private ExPluginItem _pluginItem;

        [TabGroup("Tab", "基础信息", order: 2)]
        [PropertyOrder(10)]
        [ShowInInspector]
        [HideLabel]
        public ExPluginItem PluginItem => _pluginItem;

        public PluginInformationPage(ExPluginItem pluginItem,ExMenuConfig menuConfig)
        {
            _pluginItem = pluginItem;
            _menuConfig = menuConfig;
        }

        [TabGroup("Tab", "本地信息")]
        [PropertyOrder(0)]
        [DisplayAsString(Overflow = false,EnableRichText = true)]
        [ShowInInspector]
        [HideLabel]
        private string LocalPath => $"<color=white>本地路径:{_pluginItem.LocalPath}</color>";

        [TabGroup("Tab", "本地信息")]
        [PropertyOrder(0)]
        [DisplayAsString(EnableRichText = true)]
        [ShowInInspector]
        [HideLabel]
        private string State => $"<color=orange>当前状态:{GetState()}</color>";

        [HorizontalGroup("Tab/本地信息/Buttons")]
        [Button("安装", ButtonSizes.Medium, Icon = SdfIconType.Download)]
        [HideIf(nameof(ExistPlugin))]
        public void Install()
        {
            if (ExistPlugin())
            {
                Debug.LogWarning("插件已安装，无需重复安装");
                return;
            }

            var result = EditorUtility.DisplayDialog("安装插件",
                $"是否确认安装插件 {_pluginItem.Name}？\n" +
                "安装将从远端Git仓库下载插件文件。",
                "确认", "取消");
            if (!result) return;
            
            InstallPlugin();
        }

        [HorizontalGroup("Tab/本地信息/Buttons")]
        [Button ("卸载", ButtonSizes.Medium,Icon = SdfIconType.Trash)]
        [ShowIf(nameof(ExistPlugin))]
        public void Uninstall()
        {
            if (!ExistPlugin())
            {
                Debug.LogWarning("插件未安装，无法卸载");
                return;
            }

            var result = EditorUtility.DisplayDialog("卸载插件",
                $"是否确认卸载插件 {_pluginItem.Name}？\n" +
                "卸载将删除本地插件文件夹。",
                "确认", "取消");
            if (!result) return;
            ExOpenSourceNetworkHelper.DeleteFolder(_pluginItem.LocalPath);
            Debug.Log($"插件 {_pluginItem.Name} 已卸载");
        }

        
        [HorizontalGroup("Tab/本地信息/Buttons")]
        [Button ("重装", ButtonSizes.Medium,Icon = SdfIconType.ArrowClockwise)]
        [ShowIf(nameof(ExistPlugin))]
        public void ReInstall()
        {
            var result = EditorUtility.DisplayDialog("重装插件",
                $"是否确认重装插件 {_pluginItem.Name}？\n" +
                "重装将删除当前插件并重新下载最新版本。",
                "确认", "取消");

            if (!result) return;
            // 卸载当前插件
            if (ExistPlugin())
            {
                ExOpenSourceNetworkHelper.DeleteFolder(_pluginItem.LocalPath);
                Debug.Log($"插件 {_pluginItem.Name} 已卸载");
            }

            // 重新下载插件
            InstallPlugin();
        }
        
        [HorizontalGroup("Tab/本地信息/Buttons")]
        [Button ("打开所在文件夹", ButtonSizes.Medium,Icon = SdfIconType.Folder2Open)]
        [ShowIf(nameof(ExistPlugin))]
        public void OpenFolderInExplore()
        {
            EditorUtility.RevealInFinder(_pluginItem.LocalPath);
        }


        [TitleGroup("Tab/本地信息/说明书",order:99,HideWhenChildrenAreInvisible = true)]
        [ShowInInspector]
        [ShowIf("@ExistPlugin() && !ExistPluginGuide()")]
        [HideLabel]
        [DisplayAsString(false, 16, TextAlignment.Left, true)]
        public string GuideText =>
            $"<color=orange>插件[{_pluginItem.Name}] 目录下，" +
            $"无说明书文件[{ExOpenSourceConstParam.GIT_REPO_GUIDE_FILE_NAME}]</color>";

        [TitleGroup("Tab/本地信息/说明书")]
        [ShowInInspector]
        [ShowIf("@ExistPlugin() && ExistPluginGuide()")]
        [PropertyOrder(10)]
        [Button("打开说明书", ButtonSizes.Medium, Icon = SdfIconType.Book)]
        public void OpenGuideMdInExplore()
        {
            var guideFilePath = $"{_pluginItem.LocalPath}/{ExOpenSourceConstParam.GIT_REPO_GUIDE_FILE_NAME}";
            EditorUtility.OpenWithDefaultApp(guideFilePath);
        }
        
        [TitleGroup("Tab/本地信息/说明书")]
        [ShowInInspector]
        [ShowIf("@ExistPlugin() && ExistPluginGuide()")]
        [PropertyOrder(11)]
        [HideLabel]
        [DisplayAsString(false, 12, TextAlignment.Left, true)]
        public string GuidePreview
        {
            get
            {
                if (!ExistPluginGuide()) return "";
                string fileContent = File.ReadAllText(
                    $"{_pluginItem.LocalPath}/{ExOpenSourceConstParam.GIT_REPO_GUIDE_FILE_NAME}");
                return $"<color=white>插件[{_pluginItem.Name}] 说明书预览：</color>\n" +
                       fileContent;
            }
        }

        private bool ExistPlugin()
        {
            return !ExOpenSourceNetworkHelper.IsFolderEmpty(_pluginItem.LocalPath);
        }

        private bool ExistPluginGuide()
        {
            return ExOpenSourceNetworkHelper.ExistFile($"{_pluginItem.LocalPath}/{ExOpenSourceConstParam.GIT_REPO_GUIDE_FILE_NAME}");
        }
        
        private string GetState()
        {
            return ExistPlugin() ? "已下载" : "未下载";
        }

        private GitFolderDownloadConfig GetGitFolderDownloadConfig()
        {
            return new GitFolderDownloadConfig
            {
                Username = string.IsNullOrEmpty(_pluginItem.GitURL_Username)?
                    _menuConfig.DefaultGit_UserName :
                    _pluginItem.GitURL_Username,
                Repository = string.IsNullOrEmpty(_pluginItem.GitURL_RepoName) ?
                    _menuConfig.DefaultGit_RepoName :
                    _pluginItem.GitURL_RepoName,
                Branch = string.IsNullOrEmpty(_pluginItem.GitURL_Branch) ?
                    _menuConfig.DefaultGit_Branch :
                    _pluginItem.GitURL_Branch,
                RemoteFolderPath = _pluginItem.GitURL_Path,
                LocalSavePath = _pluginItem.LocalPath
            };
        }

        private void InstallPlugin()
        {
            string user = string.IsNullOrEmpty(_pluginItem.GitURL_Username)
                ? _menuConfig.DefaultGit_UserName
                : _pluginItem.GitURL_Username;
            string repoName = string.IsNullOrEmpty(_pluginItem.GitURL_RepoName) ?
                _menuConfig.DefaultGit_RepoName :
                _pluginItem.GitURL_RepoName;
            string branch = string.IsNullOrEmpty(_pluginItem.GitURL_Branch)
                ? _menuConfig.DefaultGit_Branch
                : _pluginItem.GitURL_Branch;
            
            GitPluginUtility.DownloadPlugin(
                user,
                repoName,
                branch,
                _pluginItem.GitURL_Path,
                _pluginItem.LocalPath,
                ExOpenSourceNetworkHelper.Token());
            
            //ExOpenSourceNetworkHelper.DownloadFolder(GetGitFolderDownloadConfig());
            UpmInstaller.AddUpmPackageList(_pluginItem.Dependencies);
        }
    }
}