using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PoofLibraryManager.Editor
{
    public class PoofLibraryHostPage
    {
        // 窗口首页内容
        [BoxGroup(PoofLibraryConstParam.POOF_LIB_HOST_TITLE)] 
        [HideLabel,DisplayAsString]
        [InfoBox(PoofLibraryConstParam.POOF_LIB_HOST_MSG)]
        public string WelcomeMessage = "";

        [BoxGroup(PoofLibraryConstParam.POOF_LIB_HOST_TITLE)] 
        [HideLabel,DisplayAsString]
        [InfoBox(PoofLibraryConstParam.POOF_LIB_HOST_INTRO)]
        public string Intro = "";
        

        // [BoxGroup(PoofLibraryConstParam.POOF_LIB_HOST_TITLE)]
        // [Button("打开配置目录", ButtonSizes.Large)]
        // public void OpenConfigFolder()
        // {
        //     string directory = Path.Combine(Application.dataPath, "_PoofLibrary");
        //
        //     if (!Directory.Exists(directory))
        //     {
        //         Directory.CreateDirectory(directory);
        //         AssetDatabase.Refresh();
        //     }
        //
        //     EditorUtility.RevealInFinder(directory);
        // }
    }
}