namespace EXToyLib
{
    public enum ActivityQueueTime
    {
        Second,  // 秒
        UpdateFrame,        // 渲染帧， Update的更新频率
        FixedUpdateFrame,   // 物理帧， FixedUpdate的更新频率
        Tick,   // 自定义更新
    }
}