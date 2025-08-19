using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EXToyLib
{
    public class ActivityQueue
    {
        private readonly List<BaseActivity> _activities = new();
        private readonly ActivityQueueTime _timeType;

        public ActivityQueue(int id, ActivityQueueTime timeType = ActivityQueueTime.UpdateFrame)
        {
            ID = id;
            _timeType = timeType;
        }

        private float TimeDelta(float customDelta)
        {
            return _timeType switch
            {
                ActivityQueueTime.UpdateFrame => Time.deltaTime,
                ActivityQueueTime.FixedUpdateFrame => Time.fixedDeltaTime,
                ActivityQueueTime.CustomTick => customDelta,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public bool Running { get; private set; }
        public int ID { get; }

        public void Update(float customDelta = 0)
        {
            if (!Running) return;

            if (_activities.Count == 0) return;

            var runningActivity = _activities[0];

            if (!runningActivity.Playing)
            {
                runningActivity.StarRunning();
                runningActivity.OnStart();
            }

            runningActivity.OnTick(TimeDelta(customDelta));
            runningActivity.OnUpdate();

            if (runningActivity.IsEnd)
            {
                runningActivity.OnComplete();
                _activities.RemoveAt(0);
            }
        }

        /// <summary>
        /// 运行活动队列
        /// </summary>
        public void Run()
        {
            Running = true;
        }

        /// <summary>
        /// 暂停活动队列
        /// </summary>
        public void Stop()
        {
            Running = false;
        }
        
        /// <summary>
        /// 添加活动进队列
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="addFunction"></param>
        /// <param name="addIndex"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Add(BaseActivity activity, ActivityAddFunction addFunction = ActivityAddFunction.Last,
            int addIndex = 0)
        {
            switch (addFunction)
            {
                case ActivityAddFunction.Last:
                    _activities.Add(activity);
                    break;
                case ActivityAddFunction.First:
                {
                    _activities.Insert(_activities.Count > 0 ? 1 : 0, activity);
                    break;
                }
                case ActivityAddFunction.FirstAndInterrupt:
                {
                    if (_activities.Count > 0)
                    {
                        var runningActivity = _activities[0];
                        runningActivity.OnInterrupt();
                        _activities[0] = activity;
                    }
                    else
                    {
                        _activities.Add(activity);
                    }

                    break;
                }
                case ActivityAddFunction.FirstAndClearAll:
                {
                    if (_activities.Count > 0)
                    {
                        var runningActivity = _activities[0];
                        _activities.Clear();
                        _activities.Add(runningActivity);
                    }

                    _activities.Add(activity);
                    break;
                }
                case ActivityAddFunction.FirstAndInterruptAndClearAll:
                {
                    if (_activities.Count > 0)
                    {
                        var runningActivity = _activities[0];
                        runningActivity.OnInterrupt();
                        _activities.Clear();
                    }

                    _activities.Add(activity);
                    break;
                }
                case ActivityAddFunction.Custom:
                    addIndex = math.clamp(addIndex, 0, _activities.Count - 1);
                    _activities.Insert(addIndex, activity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 移除活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public bool Remove(BaseActivity activity)
        {
            if (activity.Playing) activity.OnInterrupt();
            return _activities.Remove(activity);
        }

        /// <summary>
        /// 通过活动ID移除活动
        /// </summary>
        /// <param name="activityID"></param>
        /// <returns></returns>
        public bool Remove(int activityID)
        {
            int index = -1;
            for (var i = 0; i < _activities.Count; i++)
            {
                var a = _activities[i];
                if (a.ID != activityID) continue;
                index = i;
                break;
            }

            if (index < 0) return false;
            _activities.RemoveAt(index);
            return true;

        }

        public bool RemoveAt(int index)
        {
            return _activities.Count > index && Remove(_activities[index]);
        }

        /// <summary>
        /// 打断当前播放的活动，跳转播放下一个活动
        /// </summary>
        public void Skip()
        {
            if(_activities.Count==0) return;
            var runningActivity = _activities[0];
            runningActivity.OnInterrupt();
            _activities.RemoveAt(0);
        }

        /// <summary>
        /// 清除活动队列
        /// </summary>
        /// <param name="interruptRunningActivity">是否打断播放中的活动</param>
        public void Clear(bool interruptRunningActivity = false)
        {
            if (interruptRunningActivity)
            {
                Skip();
                _activities.Clear();
            }
            else
            {
                var runningActivity = _activities[0];
                _activities.Clear();
                _activities.Add(runningActivity);
            }
        }
    }
}