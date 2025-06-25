using Sirenix.OdinInspector;
using UnityEngine;

namespace ExOpenSource.Editor
{
    public class ExOpenSourceHostPage
    {
        // 窗口首页内容
        [BoxGroup(ExOpenSourceConstParam.POOF_LIB_HOST_TITLE)] 
        [ShowInInspector]
        [HideLabel,DisplayAsString(false,13,TextAlignment.Left,true)]
        public string Introduction => "<color=white>" +
                                      "也许你自己平时在github开发或者发现了一些大大小小的好用插件，但每次要用的时候，又需要来来回回去github的主页收藏夹里一个一个的捞出来。" +
                                      " 捞的时候呢，有时又还不能全捞，只捞指定的一个文件夹里的东西。\n " +
                                      "我这个闲人，平时自己写插件，被这个问题来来回回困扰了不少，" +
                                      "UPM，Nuget或多或少都有自己用起来麻烦的地方，" +
                                      "或者直接说他们缺一个自定义的收藏夹。 " +
                                      "在网上也搜了不少解决方案。果不其然，" +
                                      "有不少团队为了解决这个，都自己开发了一个小型UPM。" +
                                      "不过都是不公开的，毕竟是各司内部自己开发的框架插件。" +
                                      "\n\n那既然找不到公开，我就自己快速写一个。现在国内的AI辅助也算不错了，" +
                                      "插件工具写起来已经很方便了，费不了多少时间。我就花了3个晚上的时间，把这工具给写完了。" +
                                      " 不过时间比较短，有些地方还是不够完善，后边自己用的时候会更新优化。\n \n" +
                                      "EX-HARD游戏开发交流群：616570103" +
                                      "</color>";
    }
}