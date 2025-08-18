using System.Collections.Generic;
using UnityEngine;

namespace EXToyLib
{
    public class ActivityQueueController
    {
        private static ActivityQueueController _instance;

        private ActivityQueueControllerHost _host;

        private ActivityQueueController()
        {
            Host.Init();
        }

        public static ActivityQueueController Instance => _instance ??= new ActivityQueueController();

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

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
        
        private Dictionary<int, ActivityQueue> _activityQueues = new Dictionary<int, ActivityQueue>();
        
        private Dictionary<int, ActivityQueue> _fixedUpdateActivityQueues = new Dictionary<int, ActivityQueue>();
    }
}