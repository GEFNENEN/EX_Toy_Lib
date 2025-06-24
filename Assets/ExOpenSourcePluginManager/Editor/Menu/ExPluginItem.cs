using Sirenix.OdinInspector;
using UnityEngine;

namespace ExOpenSource.Editor
{
    [System.Serializable]
    public class ExPluginItem
    {
        [BoxGroup("插件基本信息")]
        [LabelText("插件名")]
        [DisplayAsString]
        public string Name;
        
        [BoxGroup("插件基本信息")]
        [LabelText("版本")]
        [DisplayAsString]
        public string Version;
        
        [BoxGroup("插件基本信息")]
        [LabelText("简介")]
        [ReadOnly]
        [TextArea(1, 20)]
        public string Intro;
        
        [HideInInspector]
        public string MenuPath;
        
        [BoxGroup("插件基本信息")]
        [LabelText("标签")]
        [ReadOnly]
        [ShowIf("@Tags != null && Tags.Length > 0")]
        public string[] Tags;
        
        [BoxGroup("插件下载配置")]
        [LabelText("本地路径")]
        [DisplayAsString]
        public string LocalPath;
        
        [BoxGroup("插件下载配置")]
        [LabelText("远端Git仓库路径")]
        [DisplayAsString]
        public string GitURL_Path;
        
        [BoxGroup("插件下载配置")]
        [LabelText("远端Git仓库用户名")]
        [DisplayAsString]
        [ShowIf("@!string.IsNullOrEmpty(GitURL_Username)")]
        public string GitURL_Username;
        
        [BoxGroup("插件下载配置")]
        [LabelText("远端Git仓库名")]
        [DisplayAsString]
        [ShowIf("@!string.IsNullOrEmpty(GitURL_RepoName)")]
        public string GitURL_RepoName;
        
        [BoxGroup("插件下载配置")]
        [LabelText("远端Git分支名")]
        [DisplayAsString]
        [ShowIf("@!string.IsNullOrEmpty(GitURL_Branch)")]
        public string GitURL_Branch;
    }
}