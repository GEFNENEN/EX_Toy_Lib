using Sirenix.OdinInspector;

namespace PoofLibraryManager.Editor
{
    public class DownloadMenuConfig
    {
        [BoxGroup("配置下载", centerLabel: true)]
        [LabelText("Git仓库URL")]
        [Required]
        public string menuUrl => PoofLibraryConstParam.MENU_URL;
    
        [BoxGroup("配置下载")]
        [ProgressBar(0, 1, Height = 20, R = 0.2f, G = 0.7f, B = 1f)]
        [ShowIf("IsDownloading")]
        [HideLabel]
        public float downloadProgress;
    
        [BoxGroup("配置下载")]
        [ShowIf("IsDownloading")]
        [HideLabel]
        [MultiLineProperty(2)]
        public string downloadStatus = "准备下载...";
    
        [BoxGroup("配置下载")]
        [Button(ButtonSizes.Large), GUIColor(0.4f, 1f, 0.6f)]
        [DisableIf("IsDownloading")]
        [LabelText("下载默认配置")]
        public void DownloadConfig()
        {
            PoofLibraryManagerWindow.Instance.StartDownload(menuUrl);
        }
    }
}