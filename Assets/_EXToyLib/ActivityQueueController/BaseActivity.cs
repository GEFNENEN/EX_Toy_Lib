namespace EXToyLib
{
    public abstract class BaseActivity
    {
        protected float _elapsed;

        protected BaseActivity(int id, float duration)
        {
            ID = id;
            Duration = duration;
            _elapsed = 0f;
        }

        public int ID { get; }

        public float Duration { get; private set; }

        public bool Playing { get; private set; }

        public bool IsEnd => Duration>=0 && _elapsed >= Duration;

        /// <summary>
        /// 设置活动持续时间
        /// -1表示持续时间无限
        /// </summary>
        /// <param name="duration"></param>
        public void SetDuration(float duration)
        {
            Duration = duration;
        }
        
        /// <summary>
        /// 设置时间无限
        /// </summary>
        public void SetInfinite()
        {
            SetDuration(-1f);
        }
        
        /// <summary>
        /// 终结时间无限
        /// </summary>
        public void KillInfinite()
        {
            SetDuration(0);
        }

        /// <summary>
        /// 重置计时
        /// </summary>
        public void ResetElapsed()
        {
            _elapsed = 0f;
        }

        /// <summary>
        ///     计时函数
        /// </summary>
        /// <param name="delta"></param>
        public virtual void OnTick(float delta)
        {
            _elapsed += delta;
        }

        // 更新活动状态
        public virtual void OnUpdate()
        {
        }
        
        /// <summary>
        ///      活动开始时的回调
        /// </summary>
        public virtual void OnStart()
        {
        }
        
        /// <summary>
        ///      活动结束时的回调
        /// </summary>
        public virtual void OnComplete()
        {
            Dispose();
        }
        
        /// <summary>
        ///      活动被中断时的回调
        /// </summary>
        public virtual void OnInterrupt()
        {
            Dispose();
        }

        /// <summary>
        /// 析构回调
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// 开始播放活动
        /// </summary>
        public void Start()
        {
            Playing = true;
        }
    }
}