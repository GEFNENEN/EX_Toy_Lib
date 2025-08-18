namespace EXToyLib
{
    public abstract class BaseActivity
    {
        public string activityID;
        protected float duration;
        protected float elapsed;
    
        public bool IsCompleted { get { return elapsed >= duration; } }
    
        public BaseActivity(string id, float duration)
        {
            activityID = id;
            this.duration = duration;
            elapsed = 0f;
        }
    
        // 更新活动状态
        public virtual void Update(float delta)
        {
            elapsed += delta;
        }
    
        // 活动开始时的回调
        public virtual void OnStart() {}
    
        // 活动结束时的回调
        public virtual void OnComplete() {}
    
        // 重置活动状态
        public virtual void Reset()
        {
            elapsed = 0f;
        }
    }
}