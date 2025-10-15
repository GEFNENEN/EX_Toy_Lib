namespace EXToyLib
{
    public enum ActivityQueueTime
    {
        UpdateFrame, // 渲染帧， Update的更新频率
        FixedUpdateFrame, // 物理帧， FixedUpdate的更新频率
        CustomTick // 自定义更新
    }
}