using UnityEngine;

namespace EXToyLib
{
    public class ActivityQueueControllerHost : MonoBehaviour
    {
        private bool _init;

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

            ActivityQueueController.Instance.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (!_init) return;

            ActivityQueueController.Instance.OnFixedUpdate();
        }

        public void Init()
        {
            _init = true;
        }
    }
}