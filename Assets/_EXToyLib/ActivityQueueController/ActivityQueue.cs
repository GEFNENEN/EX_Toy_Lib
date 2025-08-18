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
            
            runningActivity.Update();
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
                    _activities.Insert(0,activity);
                    break;
                case ActivityAddFunction.Custom:
                    addIndex = math.clamp(addIndex, 0, _activities.Count - 1);
                    _activities.Insert(addIndex,activity);
                    break;
                default:
                    break;
            }
        }
    }
}