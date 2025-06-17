using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EXToyLib
{
    public class BezierTrajectoryController : MonoBehaviour
    {
        [Header("轨迹配置")] public BezierTrajectory trajectoryConfig = new();

        [Header("调试选项")] public bool drawGizmos = true;

        [Header("运动子弹")] public Transform bullet;

        [Header("朝向控制")] public bool alignToPath = true; // 是否沿轨迹切线方向旋转

        [Range(0, 10)] public float rotationDamping = 5.0f;

        [Header("速度曲线")] public bool useSpeedCurve = true; // 速度曲线开关

        private bool _isPlaying;

        // 结束播放事件
        private Action _onPlayEnd;

        // 开始播放事件
        private Action _onPlayStart;

        private float _startTime = -1f;

        private void Update()
        {
            if (_isPlaying) UpdateTrajectory();
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            trajectoryConfig.DrawGizmos();
        }

        public void RegisterOnPlayStart(Action action)
        {
            _onPlayStart += action;
        }

        public void UnregisterOnPlayStart(Action action)
        {
            _onPlayStart -= action;
        }

        public void RegisterOnPlayEnd(Action action)
        {
            _onPlayEnd += action;
        }

        public void UnregisterOnPlayEnd(Action action)
        {
            _onPlayEnd -= action;
        }

        // 更新轨迹预览
        public void UpdateTrajectory()
        {
            // 预览对象位置
            if (bullet == null) return;

            if (_startTime < 0f)
                trajectoryConfig.progress = 0f;
            else
                trajectoryConfig.progress = (Time.time - _startTime) / trajectoryConfig.time;

            // 应用速度曲线重映射
            var remappedProgress = useSpeedCurve
                ? trajectoryConfig.RemapProgressBySpeedCurve(trajectoryConfig.progress)
                : trajectoryConfig.progress;

            bullet.position = GetPosition(remappedProgress);

            if (alignToPath && trajectoryConfig.progress > 0.01f)
            {
                var tangent = trajectoryConfig.GetTangentAt(trajectoryConfig.progress);
                // 跳过零向量避免报错
                if (tangent.sqrMagnitude > 0.001f)
                {
                    var targetRot = Quaternion.LookRotation(tangent);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        rotationDamping * Time.deltaTime
                    );
                }
            }

            // 自动停止
            if (trajectoryConfig.progress >= 1f)
            {
                Stop();
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
            _isPlaying = true;
            _startTime = Time.time;
            _onPlayStart?.Invoke();
            UpdateTrajectory();
        }

        [Button]
        public void Stop()
        {
            _startTime = -1f;
            _isPlaying = false;
            trajectoryConfig.progress = 0f;
            _onPlayEnd?.Invoke();
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