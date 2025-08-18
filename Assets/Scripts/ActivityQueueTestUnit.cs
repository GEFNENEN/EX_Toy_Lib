using EXToyLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class ActivityQueueTestUnit : MonoBehaviour
    {
        private const int DefaultActivityQueueId = 1;
        [Button]
        public void OnInitDefaultActivityQueue()
        {
            ActivityQueueController.Instance.RegisterActivityQueue(DefaultActivityQueueId);
        }
    
        [Button]
        public void OnAddLogActivity()
        {
            var activity = new ActivityLog(1, 2.3f);
            ActivityQueueController.Instance.AddActivityToQueue(DefaultActivityQueueId,activity);
        }
    }
}