using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PoofLibraryManager.Editor
{
    [System.Serializable]
    public class PLMenuConfig
    {
        [BoxGroup("插件库基本信息")]
        [LabelText("插件库名")]
        [DisplayAsString]
        public string Name;
        
        [BoxGroup("插件库基本信息")]
        [LabelText("目录版本")]
        [DisplayAsString]
        public string Version;
        
        [BoxGroup("插件库基本信息")]
        [LabelText("作者")]
        [DisplayAsString]
        public string Owner;
        
        [BoxGroup("插件库基本信息")]
        [LabelText("插件库简介"),TextArea(1, 20)]
        [ReadOnly]
        public string Intro;
        
        [BoxGroup("Git配置")]
        [LabelText("默认Git用户名")]
        [DisplayAsString]
        public string DefaultGit_UserName;
        
        [BoxGroup("Git配置")]
        [LabelText("默认Git仓库名")]
        [DisplayAsString]
        public string DefaultGit_RepoName;
        
        [BoxGroup("Git配置")]
        [LabelText("默认Git分支名")]
        [DisplayAsString]
        public string DefaultGit_Branch;
        
        [Title("插件列表")]
        [LabelText("-")]
        [ListDrawerSettings(IsReadOnly = true)]
        public List<PLPluginItem> Plugins = new List<PLPluginItem>();
    }
}