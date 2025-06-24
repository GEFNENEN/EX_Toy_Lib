using Sirenix.OdinInspector;
using UnityEngine;

namespace ExOpenSource.Editor
{
    public class ExOpenSourceHostPage
    {
        // 窗口首页内容
        [BoxGroup(ExOpenSourceConstParam.POOF_LIB_HOST_TITLE)] 
        [ShowInInspector]
        [HideLabel,DisplayAsString(false,10,TextAlignment.Left,true)]
        public string Introduction => ExOpenSourceConstParam.POOF_LIB_HOST_INTRO;
    }
}