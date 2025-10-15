namespace EXToyLib
{
    public enum ActivityAddFunction
    {
        Last,  // 默认添加到队尾
        First, // 添加到队头,不打断正在播放的活动
        FirstAndInterrupt, // 添加到队头，打断正在播放的活动。立即播放
        FirstAndClearAll, // 添加到队头，不打断正在播放的活动，清除其他等待播放的活动。等待播放
        FirstAndInterruptAndClearAll, // 添加到队头，打断正在播放的活动，并且清除其他等待播放的活动。立即播放
        Custom, // 添加到自定义位置
    }
}