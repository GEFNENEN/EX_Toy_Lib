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
        private int _id;

        public ActivityQueue(int id, ActivityQueueTime timeType = ActivityQueueTime.UpdateFrame)
        {
            _id = id;
            _timeType = timeType;
        }

        public bool Running { get; private set; }

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

        public void Run()
        {
            Running = true;
        }

        public void Stop()
        {
            Running = false;
        }


        public void AddActivity(BaseActivity activity, ActivityAddFunction addFunction = ActivityAddFunction.Last,
            int addIndex = 0)
        {
            switch (addFunction)
            {
                case ActivityAddFunction.Last:
                    _activities.Add(activity);
                    break;
                case ActivityAddFunction.First:
                {
                    if (_activities.Count > 0)
                        _activities.Insert(1, activity);
                    else
                        _activities.Insert(0, activity);
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

        public bool RemoveActivity(BaseActivity activity)
        {
            if (activity.Playing) activity.OnInterrupt();
            return _activities.Remove(activity);
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
    }
}