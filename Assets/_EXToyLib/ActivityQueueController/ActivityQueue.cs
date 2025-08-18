using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace EXToyLib
{
    public class ActivityQueue
    {
        private int _id;
        
        private ActivityQueueTime _timeType;

        private bool _running;

        private List<BaseActivity> _activities = new List<BaseActivity>();

        public ActivityQueue(int id)
        {
            _id = id;
        }
        
        public void Update()
        {
            if(!_running) return;
            
            if(_activities.Count==0)return;

            var runningActivity = _activities[0];
            
            // TODO 
            //runningActivity.Update();
        }

        public void Run() => _running = true;
        public void Stop() => _running = false;

        public void AddActivity(BaseActivity activity,ActivityAddFunction addFunction = ActivityAddFunction.Last,int addIndex = 0)
        {
            switch (addFunction)
            {
                case ActivityAddFunction.Last:
                    _activities.Add(activity);
                    break;
                case ActivityAddFunction.First:
                {
                    if(_activities.Count>0)
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
                    _activities.Insert(addIndex,activity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool RemoveActivity(BaseActivity activity)
        {
            if (activity.Running)
            {
                activity.OnInterrupt();
            }
            return _activities.Remove(activity);
        }
    }
}