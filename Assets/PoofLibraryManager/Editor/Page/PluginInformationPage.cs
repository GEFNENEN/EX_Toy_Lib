using Sirenix.OdinInspector;

namespace PoofLibraryManager.Editor
{
    public class PluginInformationPage
    {
        [TabGroup("Tab", "基础信息")]
        [PropertyOrder(10)]
        [ShowInInspector]
        [HideLabel]
        [ReadOnly]
        private PLPluginItem _pluginItem;
        
        public PluginInformationPage(PLPluginItem pluginItem)
        {
            _pluginItem = pluginItem;
        }

        [TabGroup("Tab", "本地信息")]
        [PropertyOrder(1)]
        [Button("下载插件")]
        void Load()
        {
            //PoofLibNetworkHelper.DownloadFolder( );
        }
    }
}