using UnityEngine;

namespace EXToyLib
{
    public class ActivityQueueControllerHost : MonoBehaviour
    {
        private bool _init;
        private ActivityQueueController _ctrl;

        private void Awake()
        {
            if (FindObjectsOfType<ActivityQueueControllerHost>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }

        private void Update()
        {
            if (!_init) return;

            _ctrl.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (!_init) return;

            _ctrl.OnFixedUpdate();
        }

        public void Init(ActivityQueueController controller)
        {
            _ctrl = controller;
            _init = true;
        }
    }
}