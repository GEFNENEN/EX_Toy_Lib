using UnityEngine;
using System.Collections.Generic;

namespace EXToyLib
{
    /// <summary>
    /// 贝塞尔曲线数据逻辑类
    /// </summary>
    [System.Serializable]
    public class BezierTrajectory
    {
        [Header("基础参数")] public Vector3 startPoint;
        public Vector3 endPoint;
        [Range(0f, 1f)] public float progress = 0f;

        [Header("控制点设置")] [Tooltip("使用欧拉角自动计算控制点")]
        public bool useAngleCalculation = true;

        [Tooltip("相对于起点的偏移角度")] public Vector3 startAngle = Vector3.zero;
        [Tooltip("控制点距离起点的距离")] public float controlPointDistance = 5f;

        [Space(10)] [Tooltip("手动设置控制点")] public List<Vector3> controlPoints = new List<Vector3>();

        [Space(10)] [Tooltip("子弹运动时间")] public float time = 1f;
        
        // 计算贝塞尔曲线上的点
        public Vector3 Evaluate()
        {
            // 确保控制点数据有效
            List<Vector3> points = GetControlPoints();

            // 如果没有控制点，返回线性插值
            if (points.Count == 0)
            {
                return Vector3.Lerp(startPoint, endPoint, progress);
            }

            // 计算贝塞尔曲线点
            return CalculateBezierPoint(progress, startPoint, points, endPoint);
        }

        // 获取有效的控制点列表
        private List<Vector3> GetControlPoints()
        {
            List<Vector3> validPoints = new List<Vector3>();

            // 使用角度计算控制点
            if (useAngleCalculation)
            {
                Quaternion rotation = Quaternion.Euler(startAngle);
                Vector3 direction = rotation * Vector3.forward;
                validPoints.Add(startPoint + direction * controlPointDistance);
            }

            // 添加手动设置的控制点（最多3个）
            int maxPoints = 3 - validPoints.Count;
            for (int i = 0; i < controlPoints.Count && i < maxPoints; i++)
            {
                validPoints.Add(controlPoints[i]);
            }

            return validPoints;
        }

        // 计算贝塞尔曲线点（递归方法）
        private Vector3 CalculateBezierPoint(float t, params Vector3[] points)
        {
            if (points.Length == 1)
            {
                return points[0];
            }

            Vector3[] newPoints = new Vector3[points.Length - 1];
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = Vector3.Lerp(points[i], points[i + 1], t);
            }

            return CalculateBezierPoint(t, newPoints);
        }

        // 计算贝塞尔曲线点（迭代方法）
        private Vector3 CalculateBezierPoint(float t, Vector3 p0, List<Vector3> cps, Vector3 pn)
        {
            List<Vector3> allPoints = new List<Vector3> { p0 };
            allPoints.AddRange(cps);
            allPoints.Add(pn);

            Vector3[] points = allPoints.ToArray();
            int n = points.Length - 1;

            Vector3 result = Vector3.zero;
            for (int i = 0; i <= n; i++)
            {
                float coefficient = BinomialCoefficient(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
                result += points[i] * coefficient;
            }

            return result;
        }

        // 计算二项式系数（n选k）
        private int BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            if (k == 0 || k == n) return 1;

            int result = 1;
            k = Mathf.Min(k, n - k);

            for (int i = 1; i <= k; i++)
            {
                result *= n - (k - i);
                result /= i;
            }

            return result;
        }

        // 在场景中绘制轨迹预览
        public void DrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint, 0.15f);

            List<Vector3> points = GetControlPoints();
            Gizmos.color = Color.yellow;
            foreach (Vector3 point in points)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            // 绘制完整曲线
            Gizmos.color = Color.cyan;
            Vector3 prevPoint = startPoint;
            int segments = 30;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 currentPoint = CalculateBezierPoint(t, startPoint, points, endPoint);
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }

            // 绘制当前进度点
            Vector3 currentPos = Evaluate();
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentPos, 0.17f);
        }
    }
}