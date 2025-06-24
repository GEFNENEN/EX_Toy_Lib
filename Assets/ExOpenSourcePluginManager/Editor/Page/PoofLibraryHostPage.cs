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
        [ShowInInspector]
        [HideLabel,DisplayAsString(false,14,TextAlignment.Left,true)]
        public string Introduction => PoofLibraryConstParam.POOF_LIB_HOST_INTRO;
    }
}