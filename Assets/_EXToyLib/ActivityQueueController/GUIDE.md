# ActivityQueueController 活动队列控制器
## 简介
自工作以来，遇到了很多次关于时序执行一些活动的需求。比如UI里的悬浮Tip，多条提示时要求装入队列一条一条的展示；相机的机位运动，分段执行；播放歌曲列表；等等。
我索性把活动队列抽象出来，整合为了一个更加通用的小框架，方便拓展和使用。

## 相关类与接口说明
### ActivityQueueTime
- 活动队列计时类型枚举
- UpdateFrame : 渲染帧， Update更新频率
- FixedUpdateFrame : 物理帧， FixedUpdate更新频率
- CustomTick : 自定义更新频率
### ActivityAddFunction
- 活动添加方式类型枚举
-  Last: 默认添加到队尾
-  First: 添加到队头,不打断正在播放的活动
-  FirstAndInterrupt: 添加到队头，打断正在播放的活动。立即播放
-  FirstAndClearAll: 添加到队头，不打断正在播放的活动，清除其他等待播放的活动。等待播放
-  FirstAndInterruptAndClearAll: 添加到队头，打断正在播放的活动，并且清除其他等待播放的活动。立即播放
-  Custom: 添加到自定义位置
### BaseActivity
- 活动抽象基类。活动就是指执行的具体业务，比如播悬浮Tip，相机运镜，人物行为，等等。
  活动的具体逻辑通过重载以下5个函数即可：
    - OnStart() 开始回调
    - OnUpdate() 更新回调
    - OnComplete() 完成回调
    - OnInterrupt() 被打断回调
    - Dispose() 析构回调
- ID  活动ID，通常用ActivityQueueController.GenerateNewActivityID()生成即可，如果想自己管理也可以使用自己生成的ID
- Duration 活动持续时间（单位：秒）。 特别注意：负数值表示持续时间无限
- Playing 活动是否播放中
- IsEnd 活动是否播放结束
- SetDuration(float duration)  设置持续时间
- SetInfinite()  设置时间无限
- KillInfinite()  终结时间无限
- ResetElapsed()  重置计时
- OnTick(float delta)   计时器函数，运行自定义重载。
### ActivityQueue
- 活动队列类。虽然叫队列，但其实不是Queue结构。因为实际应用里有许多，打断，插值，移除的操作需要，所以用的是List结构。
- ID 活动队列ID
- Running 是否运行中
- Run() 运行活动队列
- Stop() 暂停活动队列
- Add(BaseActivity activity, ActivityAddFunction addFunction = ActivityAddFunction.Last, int addIndex = 0) 添加活动进队列
  - addFunction 添加方式
  - addIndex 添加位置。  如果是自定义位置添加，传入添加位置
- Remove(int activityID) 通过活动ID移除活动
- RemoveAt(int index) 通过活动排序索引移除活动
- Skip()  打断当前播放的活动，跳转播放下一个活动
- Clear(bool interruptRunningActivity = false)  清除活动队列.   
  - interruptRunningActivity:是否打断播放中的活动
### ActivityQueueController
- 活动队列控制器类。 实际这个小框架用的最多的类。接口我尽量简化了，为了方便使用。
- RegisterQueue(int id, ActivityQueueTime timeType = ActivityQueueTime.UpdateFrame) 注册活动队列
  - id 队列ID
  - timeType 队列计时类型，默认是update更新类型
- UnregisterQueue(int id) 注销活动队列
- GetQueue(int id) 获取活动队列
- RunQueue(int id) 运行活动队列
- StopQueue(int id) 暂停活动队列
- AddActivity(int queueId, BaseActivity activity, ActivityAddFunction addFunction = ActivityAddFunction.Last, int addIndex = -1) 添加活动
  - queueId 要添加的队列ID
  - activity 添加的活动
  - addFunction 添加方式
  - addIndex 添加位置。  如果是自定义位置添加，传入添加位置
- ClearQueue(int id,bool interruptRunningActivity = false) 清空活动队列
  - id 活动队列ID
  - interruptRunningActivity 是否打断播放中的活动
- GenerateNewActivityID() 默认活动ID生成函数， 可以自定活动ID
- OnCustomUpdate(float customDelta) 自定义更新频率函数，如果ActivityQueueTime有CustomTick类型，则需要自己找合适的时机调用该函数进行更新。


## 使用案例
自定义一个简单的日志输出的活动类型
```
public class ActivityLog : BaseActivity
{
    public ActivityLog(int id, float duration) : base(id, duration)
    {
    }
    
    public override void OnStart()
    {
        Debug.Log($"Activity {ID} started with duration {Duration}");
    }
    
    public override void OnUpdate()
    {
        Debug.Log($"Activity {ID} is updating. Elapsed time: {_elapsed}");
    }
    
    public override void OnComplete()
    {
        Debug.Log($"Activity {ID} completed after {_elapsed} seconds.");
        base.OnComplete();
    }

    public override void OnInterrupt()
    {
        Debug.Log($"Activity {ID} interrupt after {_elapsed} seconds.");
        base.OnInterrupt();
    }
}
```

然后初始化活动队列
```
private const int DefaultActivityQueueId = 1;
ActivityQueueController.Instance.RegisterQueue(DefaultActivityQueueId);
```

添加测试用的活动
```
var activity1 = new ActivityLog(ActivityQueueController.GenerateNewActivityID(), 2.3f);
ActivityQueueController.Instance.AddActivity(DefaultActivityQueueId,activity1);

var activity2 = new ActivityLog(ActivityQueueController.GenerateNewActivityID(), 1f);
ActivityQueueController.Instance.AddActivity(DefaultActivityQueueId,activity2);

var activity3 = new ActivityLog(ActivityQueueController.GenerateNewActivityID(), 5f);
ActivityQueueController.Instance.AddActivity(DefaultActivityQueueId,activity3);
```

其余常用接口的使用：
```
var q = ActivityQueueController.Instance.GetQueue(DefaultActivityQueueId);

// 清除
q?.Clear(true);
// 切歌（打断）
q?.Skip();
```