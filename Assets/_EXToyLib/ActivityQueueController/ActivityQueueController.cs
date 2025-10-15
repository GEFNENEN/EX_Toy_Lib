using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXToyLib
{
    public class ActivityQueueController
    {
        private static ActivityQueueController _instance;

        private readonly Dictionary<int, ActivityQueue> _customUpdateActivityQueues = new();

        private readonly Dictionary<int, ActivityQueue> _fixedUpdateActivityQueues = new();

        private readonly Dictionary<int, ActivityQueue> _normalActivityQueues = new();

        private ActivityQueueControllerHost _host;

        private readonly Dictionary<int, ActivityQueueTime> _id2TimeType = new();

        private ActivityQueueController()
        {
            Host.Init(this);
        }

        public static ActivityQueueController Instance
        {
            get
            {
                 _instance ??= new ActivityQueueController();
                 _instance.CheckHost();
                 return _instance;
            }
        }

        private ActivityQueueControllerHost Host
        {
            get
            {
                if (_host != null) return _host;
                var go = new GameObject("ActivityQueueControllerHost");
                _host = go.AddComponent<ActivityQueueControllerHost>();
                return _host;
            }
        }

        private void CheckHost()
        {
            if(_host==null) Host.Init(this);
        }
        
        public void OnUpdate()
        {
            foreach (var (_, v) in _normalActivityQueues)
                if (v.Running)
                    v.Update();
        }

        public void OnFixedUpdate()
        {
            foreach (var (_, v) in _fixedUpdateActivityQueues)
                if (v.Running)
                    v.Update();
        }

        public void OnCustomUpdate(float customDelta)
        {
            foreach (var (_, v) in _customUpdateActivityQueues)
                if (v.Running)
                    v.Update(customDelta);
        }

        /// <summary>
        /// 注册活动队列
        /// 注册后的活动队列，是默认运行的
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void RegisterQueue(int id, ActivityQueueTime timeType = ActivityQueueTime.UpdateFrame)
        {
            if (_id2TimeType.ContainsKey(id))
            {
                Debug.LogWarning($"ActivityQueue with ID {id} already exists.");
                return;
            }

            var queue = timeType switch
            {
                ActivityQueueTime.UpdateFrame => new ActivityQueue(id),
                ActivityQueueTime.FixedUpdateFrame => new ActivityQueue(id, ActivityQueueTime.FixedUpdateFrame),
                ActivityQueueTime.CustomTick => new ActivityQueue(id, ActivityQueueTime.CustomTick),
                _ => throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null)
            };

            // 注册后自动运行
            queue.Run();
            
            switch (timeType)
            {
                case ActivityQueueTime.FixedUpdateFrame:
                    _fixedUpdateActivityQueues[id] = queue;
                    break;
                case ActivityQueueTime.CustomTick:
                    _customUpdateActivityQueues[id] = queue;
                    break;
                default:
                    _normalActivityQueues[id] = queue;
                    break;
            }

            _id2TimeType[id] = timeType;
        }

        /// <summary>
        /// 注销活动队列
        /// </summary>
        /// <param name="id"></param>
        public void UnregisterQueue(int id)
        {
            if (_normalActivityQueues.ContainsKey(id))
            {
                _normalActivityQueues[id].Clear(true);
                _normalActivityQueues.Remove(id);
            }
            else if (_fixedUpdateActivityQueues.ContainsKey(id))
            {
                _fixedUpdateActivityQueues[id].Clear(true);
                _fixedUpdateActivityQueues.Remove(id);
            }
            else if (_customUpdateActivityQueues.ContainsKey(id))
            {
                _customUpdateActivityQueues[id].Clear(true);
                _customUpdateActivityQueues.Remove(id);
            }
            else
                Debug.LogWarning($"No ActivityQueue found with ID {id}.");

            _id2TimeType.Remove(id);
        }

        /// <summary>
        /// 获取活动队列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ActivityQueue GetQueue(int id)
        {
            if (_id2TimeType.TryGetValue(id, out var timeType))
                switch (timeType)
                {
                    case ActivityQueueTime.UpdateFrame:
                        if (_normalActivityQueues.TryGetValue(id, out var updateFrameQueue))
                            return updateFrameQueue;
                        break;
                    case ActivityQueueTime.FixedUpdateFrame:
                        if (_fixedUpdateActivityQueues.TryGetValue(id, out var fixedQueue))
                            return fixedQueue;
                        break;
                    case ActivityQueueTime.CustomTick:
                        if (_customUpdateActivityQueues.TryGetValue(id, out var customQueue))
                            return customQueue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            Debug.LogWarning($"No ActivityQueue found with ID {id}.");
            return null;
        }

        /// <summary>
        /// 运行队列
        /// </summary>
        /// <param name="id"></param>
        public void RunQueue(int id)
        {
            var queue = GetQueue(id);
            queue?.Run();
        }
        
        /// <summary>
        /// 暂停队列
        /// </summary>
        /// <param name="id"></param>
        public void StopQueue(int id)
        {
            var queue = GetQueue(id);
            queue?.Stop();
        }
        
        /// <summary>
        /// 添加活动
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="activity"></param>
        /// <param name="addFunction"></param>
        /// <param name="addIndex"></param>
        public void AddActivity(int queueId, BaseActivity activity,
            ActivityAddFunction addFunction = ActivityAddFunction.Last, int addIndex = -1)
        {
            var queue = GetQueue(queueId);
            if (queue == null)
            {
                Debug.LogWarning($"No ActivityQueue found with ID {queueId}.");
                return;
            }

            queue.Add(activity, addFunction, addIndex);
        }

        /// <summary>
        /// 清空活动队列
        /// </summary>
        /// <param name="id">活动队列ID</param>
        /// <param name="interruptRunningActivity">是否打断播放中的活动</param>
        public void ClearQueue(int id,bool interruptRunningActivity = false)
        {
            var queue = GetQueue(id);
            queue?.Clear(interruptRunningActivity);
        }

        #region Default Activity ID Generator

        private static int CURRENT_ACTIVITY_GEN_ID = 0;

        /// <summary>
        /// 默认活动ID生成函数， 可以自定活动ID
        /// </summary>
        /// <returns></returns>
        public static int GenerateNewActivityID()
        {
            return CURRENT_ACTIVITY_GEN_ID++;
        }
        #endregion
    }
}