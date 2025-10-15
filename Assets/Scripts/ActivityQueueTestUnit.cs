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
            ActivityQueueController.Instance.RegisterQueue(DefaultActivityQueueId);
        }
    
        [Button]
        public void OnAddLogActivity()
        {
            var activity = new ActivityLog(ActivityQueueController.GenerateNewActivityID(), 2.3f);
            ActivityQueueController.Instance.AddActivity(DefaultActivityQueueId,activity);
        }
        
        [Button]
        public void ClearAllWithInterrupt()
        {
            var q = ActivityQueueController.Instance.GetQueue(DefaultActivityQueueId);
            q.Clear(true);
        }
        
        [Button]
        public void ClearAll()
        {
            var q = ActivityQueueController.Instance.GetQueue(DefaultActivityQueueId);
            q.Clear();
        }
        
        [Button]
        public void Skip()
        {
            var q = ActivityQueueController.Instance.GetQueue(DefaultActivityQueueId);
            q.Skip();
        }
    }
}