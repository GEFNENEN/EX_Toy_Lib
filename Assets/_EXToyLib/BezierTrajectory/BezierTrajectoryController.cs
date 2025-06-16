using Sirenix.OdinInspector;
using UnityEngine;

namespace EXToyLib
{
    public class BezierTrajectoryController : MonoBehaviour
    {
        [Header("轨迹配置")] public BezierTrajectory trajectoryConfig = new();

        [Header("调试选项")] public bool drawGizmos = true;

        [Header("运动子弹")] public Transform bullet;
        private bool _isPlaying;

        private float _startTime = -1f;

        private void Update()
        {
            if (_isPlaying) UpdateTrajectory();
        }

        // 在场景中绘制轨迹
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            trajectoryConfig.DrawGizmos();
        }

        // 更新轨迹预览
        public void UpdateTrajectory()
        {
            // 预览对象位置
            if (bullet != null)
            {
                if (_startTime < 0f)
                    trajectoryConfig.progress = 0f;
                else
                    trajectoryConfig.progress = (Time.time - _startTime) / trajectoryConfig.time;
                bullet.position = GetPosition(trajectoryConfig.progress);
            }
        }

        // 获取指定进度的位置
        public Vector3 GetPosition(float progress)
        {
            trajectoryConfig.progress = Mathf.Clamp01(progress);
            return trajectoryConfig.Evaluate();
        }

        [Button]
        public void Play()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            _startTime = Time.time;
            UpdateTrajectory();
        }

        [Button]
        public void Stop()
        {
            _startTime = -1f;
            _isPlaying = false;
            trajectoryConfig.progress = 0f;
            UpdateTrajectory();
        }

        [Button]
        public void Pause()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            UpdateTrajectory();
        }

        [Button]
        public void Continue()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            _startTime = Time.time - trajectoryConfig.progress * trajectoryConfig.time;
            UpdateTrajectory();
        }


#if UNITY_EDITOR
        [ContextMenu("自动对齐起点终点")]
        private void AutoAlignPoints()
        {
            if (transform.childCount >= 2)
            {
                trajectoryConfig.startPoint = transform.GetChild(0).position;
                trajectoryConfig.endPoint = transform.GetChild(1).position;
            }
        }
#endif
    }
}