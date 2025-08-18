namespace EXToyLib
{
    public abstract class BaseActivity
    {
        protected float elapsed;

        public BaseActivity(int id, float duration)
        {
            ID = id;
            Duration = duration;
            elapsed = 0f;
        }

        public int ID { get; }

        public float Duration { get; private set; }

        public bool Playing { get; private set; }

        public bool IsEnd => elapsed >= Duration;

        public void SetDuration(float duration)
        {
            Duration = duration;
        }

        public void ResetElapsed()
        {
            elapsed = 0f;
        }

        /// <summary>
        ///     计时
        /// </summary>
        /// <param name="delta"></param>
        public virtual void OnTick(float delta)
        {
            elapsed += delta;
        }

        // 更新活动状态
        public virtual void OnUpdate()
        {
        }

        // 活动开始时的回调
        public virtual void OnStart()
        {
        }

        // 活动结束时的回调
        public virtual void OnComplete()
        {
            Dispose();
        }

        // 活动被中断时的回调
        public virtual void OnInterrupt()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
        }

        public void StarRunning()
        {
            Playing = true;
        }
    }
}