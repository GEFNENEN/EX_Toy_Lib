using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SecondOrderDynamicsComponent))]
public class SecondOrderDynamicsComponentEditor : Editor
{
    private SecondOrderDynamicsComponent _target;
    private List<Vector2> _rawPoints = new List<Vector2>();
    private List<Vector2> _smoothedPoints = new List<Vector2>();
    private float _totalTime = 3f;
    private float _timeStep = 0.01f;
    private int _pointCount;
    private Rect _curveRect;
    private Color _rawColor = new Color(1, 0.3f, 0.3f);
    private Color _smoothedColor = new Color(0.3f, 0.7f, 1);
    private float _lineWidth = 1.5f;
    
    // 动态计算的范围值
    private float _minY = 0f;
    private float _maxY = 1.5f;
    private float _minX = 0f;
    private float _maxX = 3f;

    void OnEnable()
    {
        _target = (SecondOrderDynamicsComponent)target;
        _pointCount = Mathf.RoundToInt(_totalTime / _timeStep) + 1;
        UpdateCurveData();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        // 检测参数变化
        bool paramChanged = false;
        EditorGUI.BeginChangeCheck();
        _target.frequency = EditorGUILayout.Slider("频率 (Hz)", _target.frequency, 0.1f, 7f);
        _target.damping = EditorGUILayout.Slider("阻尼比", _target.damping, 0f, 1f);
        _target.scale = EditorGUILayout.Slider("缩放因子", _target.scale, -10f, 10f);
        if (EditorGUI.EndChangeCheck())
        {
            paramChanged = true;
        }

        // 参数变化时更新曲线
        if (paramChanged)
        {
            UpdateCurveData();
        }

        // 绘制曲线预览区域
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("阶跃响应曲线预览", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"X轴范围: {_minX:F2}-{_maxX:F2}s | Y轴范围: {_minY:F2}-{_maxY:F2}", MessageType.Info);
        
        _curveRect = GUILayoutUtility.GetRect(400, 200);
        
        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(_curveRect);
            GL.PushMatrix();
            GL.Clear(true, false, new Color(0.15f, 0.15f, 0.15f));
            DrawCurvePreview(new Rect(0, 0, _curveRect.width, _curveRect.height));
            GL.PopMatrix();
            GUI.EndClip();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateCurveData()
    {
        _rawPoints.Clear();
        _smoothedPoints.Clear();

        // 初始化动力学系统
        SecondOrderDynamics dynamics = new SecondOrderDynamics(
            _target.frequency,
            _target.damping,
            _target.scale,
            Vector3.zero
        );

        // 重置动力学状态
        dynamics.Reset(Vector3.zero);

        // 计算曲线点
        for (int i = 0; i < _pointCount; i++)
        {
            float t = i * _timeStep;
            float deltaTime = _timeStep;

            // 原始值（阶跃函数）
            float rawValue = t < 1f ? 0f : 1f;
            _rawPoints.Add(new Vector2(t, rawValue));

            // 计算平滑值
            Vector3 smoothedValue = dynamics.Update(
                deltaTime,
                new Vector3(rawValue, 0, 0),
                null
            );
            _smoothedPoints.Add(new Vector2(t, smoothedValue.x));
        }

        // 计算动态范围
        CalculateDynamicRange();
    }

    /// <summary>
    /// 根据曲线数据计算动态坐标范围
    /// </summary>
    private void CalculateDynamicRange()
    {
        // 计算X轴范围（固定为0到总时间）
        _minX = 0f;
        _maxX = _totalTime;
        
        // 计算Y轴范围（包含原始值和平滑值）
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        // 查找所有点中的最小值和最大值
        foreach (var point in _rawPoints.Concat(_smoothedPoints))
        {
            if (point.y < minY) minY = point.y;
            if (point.y > maxY) maxY = point.y;
        }
        
        // 添加边距（10%的额外空间）
        float yMargin = (maxY - minY) * 0.1f;
        minY -= yMargin;
        maxY += yMargin;
        
        // 确保Y轴有最小范围（避免所有值相同时显示为一条线）
        if (Mathf.Approximately(minY, maxY))
        {
            minY -= 0.5f;
            maxY += 0.5f;
        }
        
        // 确保Y轴最小值不小于0
        minY = Mathf.Min(minY, 0);
        
        _minY = minY;
        _maxY = maxY;
    }

    private void DrawCurvePreview(Rect rect)
    {
        // 绘制背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
        
        // 绘制坐标轴
        DrawAxis(rect);
        
        // 绘制两条曲线
        DrawCurve(rect, _rawPoints, _rawColor);
        DrawCurve(rect, _smoothedPoints, _smoothedColor);
        
        // 绘制图例
        DrawLegend(rect);
    }

    private void DrawCurve(Rect rect, List<Vector2> points, Color color)
    {
        if (points.Count < 2) return;

        Handles.color = color;
        Vector3 prevPoint = ToGUIPoint(rect, points[0]);
        
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 currentPoint = ToGUIPoint(rect, points[i]);
            Handles.DrawLine(prevPoint, currentPoint, _lineWidth);
            prevPoint = currentPoint;
        }
    }

    /// <summary>
    /// 将数据点转换为GUI空间中的点（基于动态范围）
    /// </summary>
    private Vector3 ToGUIPoint(Rect rect, Vector2 point)
    {
        // 计算X坐标（时间）
        float normalizedX = (point.x - _minX) / (_maxX - _minX);
        float x = rect.x + normalizedX * rect.width;
        
        // 计算Y坐标（值），注意Y轴向上为正
        float normalizedY = (point.y - _minY) / (_maxY - _minY);
        float y = rect.y + rect.height * (1 - normalizedY);
        
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// 绘制坐标轴（基于动态范围）
    /// </summary>
    private void DrawAxis(Rect rect)
    {
        // 坐标轴颜色
        Handles.color = Color.gray;

        // 绘制X轴和Y轴
        Handles.DrawLine(
            new Vector3(rect.x, rect.y + rect.height, 0),
            new Vector3(rect.x + rect.width, rect.y + rect.height, 0)
        );
        Handles.DrawLine(
            new Vector3(rect.x, rect.y, 0),
            new Vector3(rect.x, rect.y + rect.height, 0)
        );

        // X轴刻度（时间）
        int xLabels = 4; // 4个主要刻度
        for (int i = 0; i <= xLabels; i++)
        {
            float t = _minX + (_maxX - _minX) * (float)i / xLabels;
            float normalizedX = (t - _minX) / (_maxX - _minX);
            float x = rect.x + normalizedX * rect.width;
            
            // 刻度线
            Handles.DrawLine(
                new Vector3(x, rect.y + rect.height - 3, 0), 
                new Vector3(x, rect.y + rect.height + 3, 0)
            );
            
            // 刻度标签
            GUI.Label(
                new Rect(x - 15, rect.y + rect.height + 5, 30, 15), 
                t.ToString("F1"), 
                EditorStyles.miniLabel
            );
        }

        // Y轴刻度（值）
        int yLabels = 4; // 4个主要刻度
        for (int i = 0; i <= yLabels; i++)
        {
            float value = _minY + (_maxY - _minY) * (float)i / yLabels;
            float normalizedY = (value - _minY) / (_maxY - _minY);
            float y = rect.y + rect.height * (1 - normalizedY);
            
            // 刻度线
            Handles.DrawLine(
                new Vector3(rect.x - 3, y, 0), 
                new Vector3(rect.x + 3, y, 0)
            );
            
            // 刻度标签
            GUI.Label(
                new Rect(rect.x - 40, y - 8, 35, 15), 
                value.ToString("F1"), 
                EditorStyles.miniLabel
            );
        }

        // 轴标签
        GUI.Label(
            new Rect(rect.x + rect.width / 2 - 30, rect.y + rect.height + 20, 60, 20), 
            "时间 (s)", 
            EditorStyles.miniBoldLabel
        );
        GUI.Label(
            new Rect(rect.x - 35, rect.y - 20, 60, 20), 
            "值", 
            EditorStyles.miniBoldLabel
        );
    }

    private void DrawLegend(Rect rect)
    {
        // 原始值图例
        Rect rawRect = new Rect(rect.x + 10, rect.y + 10, 120, 20);
        EditorGUI.DrawRect(new Rect(rawRect.x, rawRect.y + 8, 15, 2), _rawColor);
        GUI.Label(
            new Rect(rawRect.x + 20, rawRect.y, 100, 20), 
            "原始值", 
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = _rawColor } }
        );

        // 平滑值图例
        Rect smoothedRect = new Rect(rect.x + 10, rawRect.y + 20, 120, 20);
        EditorGUI.DrawRect(new Rect(smoothedRect.x, smoothedRect.y + 8, 15, 2), _smoothedColor);
        GUI.Label(
            new Rect(smoothedRect.x + 20, smoothedRect.y, 100, 20), 
            "平滑值", 
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = _smoothedColor } }
        );
    }
}