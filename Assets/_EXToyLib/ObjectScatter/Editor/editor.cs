using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshPointGenerator : EditorWindow
{
    [System.Serializable]
    public class TargetObject
    {
        public GameObject gameObject;
        public bool isSelected = true;
        public int pointCount = 0;
    }

    private List<TargetObject> targetObjects = new List<TargetObject>();
    private Vector2 scrollPosition;
    private float density = 0.5f;
    private bool showPreview = true;
    private bool placeAsChildren = true;
    private Vector3 pointOffset = Vector3.zero;
    private float pointSize = 0.1f;
    private Color previewColor = Color.cyan;
    
    private List<GameObject> generatedPoints = new List<GameObject>();
    private bool isGenerating = false;

    [MenuItem("Tools/Mesh 表面点生成器")]
    public static void ShowWindow()
    {
        GetWindow<MeshPointGenerator>("Mesh点生成器");
    }

    void OnGUI()
    {
        GUILayout.Label("目标对象选择", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("在此添加要生成点的网格对象", MessageType.Info);
        
        // 目标对象列表
        GUILayout.BeginVertical("box");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        
        if (targetObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("还没有添加任何对象", MessageType.Warning);
        }
        
        for (int i = 0; i < targetObjects.Count; i++)
        {
            GUILayout.BeginHorizontal();
            
            targetObjects[i].isSelected = EditorGUILayout.Toggle(targetObjects[i].isSelected, GUILayout.Width(20));
            
            EditorGUI.BeginDisabledGroup(true);
            targetObjects[i].gameObject = (GameObject)EditorGUILayout.ObjectField(
                targetObjects[i].gameObject, 
                typeof(GameObject), 
                true,
                GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
            
            targetObjects[i].pointCount = EditorGUILayout.IntField(
                targetObjects[i].pointCount, 
                GUILayout.Width(60));
            
            if (GUILayout.Button("移除", GUILayout.Width(60)))
            {
                targetObjects.RemoveAt(i);
                i--;
            }
            
            GUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("添加选中对象"))
        {
            AddSelectedObjects();
        }
        
        if (GUILayout.Button("清空列表"))
        {
            targetObjects.Clear();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        
        GUILayout.Space(10);
        GUILayout.Label("点阵设置", EditorStyles.boldLabel);
        density = EditorGUILayout.Slider("点密度 (点/单位面积)", density, 0.01f, 5f);
        pointOffset = EditorGUILayout.Vector3Field("点位置偏移", pointOffset);
        
        GUILayout.Space(10);
        GUILayout.Label("预览设置", EditorStyles.boldLabel);
        showPreview = EditorGUILayout.Toggle("显示场景预览", showPreview);
        previewColor = EditorGUILayout.ColorField("预览颜色", previewColor);
        pointSize = EditorGUILayout.Slider("点尺寸", pointSize, 0.01f, 0.5f);
        
        GUILayout.Space(10);
        placeAsChildren = EditorGUILayout.Toggle("作为子物体生成", placeAsChildren);
        
        GUILayout.Space(20);
        
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(!HasSelectedTargets() || isGenerating);
        if (GUILayout.Button("生成点阵", GUILayout.Height(40)))
        {
            GeneratePointsOnSelectedObjects();
        }
        EditorGUI.EndDisabledGroup();
        
        if (GUILayout.Button("更新预览", GUILayout.Height(40)))
        {
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
        
        if (generatedPoints.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(isGenerating || generatedPoints.Count == 0);
            if (GUILayout.Button("清除所有生成的点", GUILayout.Height(30)))
            {
                ClearGeneratedPoints();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        if (isGenerating)
        {
            EditorGUILayout.HelpBox("正在生成点...", MessageType.Info);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!showPreview) return;
        
        Handles.color = previewColor;
        
        foreach (var target in targetObjects)
        {
            if (!target.isSelected || target.gameObject == null) continue;
            
            MeshFilter meshFilter = target.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;
            
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            
            float totalArea = CalculateMeshArea(mesh);
            int previewPoints = Mathf.RoundToInt(totalArea * density);
            
            Handles.matrix = target.gameObject.transform.localToWorldMatrix;
            
            for (int i = 0; i < previewPoints; i++)
            {
                Vector3 point = GetRandomPointOnMesh(mesh) + pointOffset;
                Handles.SphereHandleCap(0, point, Quaternion.identity, pointSize, EventType.Repaint);
            }
        }
    }

    private bool HasSelectedTargets()
    {
        return targetObjects.Any(t => t.isSelected && t.gameObject != null && 
                                     t.gameObject.GetComponent<MeshFilter>() != null);
    }

    private void AddSelectedObjects()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<MeshFilter>() == null) continue;
            if (targetObjects.Any(t => t.gameObject == obj)) continue;
            
            targetObjects.Add(new TargetObject {
                gameObject = obj,
                isSelected = true
            });
        }
    }

    private Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        float totalArea = CalculateMeshArea(mesh);
        
        float randomValue = Random.value * totalArea;
        float cumulative = 0;
        int triIndex = 0;
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            
            float area = Vector3.Cross(p2 - p1, p3 - p1).magnitude * 0.5f;
            cumulative += area;
            
            if (randomValue <= cumulative)
            {
                triIndex = i;
                break;
            }
        }
        
        Vector3 p1_t = vertices[triangles[triIndex]];
        Vector3 p2_t = vertices[triangles[triIndex + 1]];
        Vector3 p3_t = vertices[triangles[triIndex + 2]];
        
        return GetRandomPointInTriangle(p1_t, p2_t, p3_t);
    }

    private float CalculateMeshArea(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        float totalArea = 0;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            
            totalArea += Vector3.Cross(p2 - p1, p3 - p1).magnitude * 0.5f;
        }
        
        return totalArea;
    }

    private void GeneratePointsOnSelectedObjects()
    {
        ClearGeneratedPoints();
        
        var selectedTargets = targetObjects
            .Where(t => t.isSelected && t.gameObject != null && 
                        t.gameObject.GetComponent<MeshFilter>() != null)
            .ToList();
        
        if (selectedTargets.Count == 0) return;
        
        isGenerating = true;
        
        try
        {
            int totalObjects = selectedTargets.Count;
            int currentObjectIndex = 0;
            
            foreach (var target in selectedTargets)
            {
                currentObjectIndex++;
                
                // 更新进度条显示当前对象
                string progressTitle = $"生成点阵 ({currentObjectIndex}/{totalObjects})";
                string progressInfo = $"正在处理: {target.gameObject.name}";
                
                // 计算当前对象需要生成的点数
                Mesh mesh = target.gameObject.GetComponent<MeshFilter>().sharedMesh;
                float totalArea = CalculateMeshArea(mesh);
                int totalPoints = Mathf.RoundToInt(totalArea * density);
                
                // 更新目标对象的点数统计
                target.pointCount = totalPoints;
                
                Transform parent = placeAsChildren ? target.gameObject.transform : null;
                
                // 生成点阵
                for (int i = 0; i < totalPoints; i++)
                {
                    // 更新点生成进度
                    float pointProgress = (float)i / totalPoints;
                    
                    // 检查用户是否取消了操作
                    if (EditorUtility.DisplayCancelableProgressBar(
                        progressTitle, 
                        $"{progressInfo}\n点: {i + 1}/{totalPoints}",
                        pointProgress))
                    {
                        // 用户点击了取消按钮
                        Debug.Log("点阵生成已被用户取消");
                        return;
                    }
                    
                    Vector3 localPoint = GetRandomPointOnMesh(mesh) + pointOffset;
                    Vector3 worldPoint = target.gameObject.transform.TransformPoint(localPoint);
                    
                    // 创建点对象
                    GameObject pointObj = new GameObject($"Point_{target.gameObject.name}_{i}");
                    Undo.RegisterCreatedObjectUndo(pointObj, "Create Point");
                    
                    pointObj.transform.position = worldPoint;
                    pointObj.transform.SetParent(parent);
                    
                    generatedPoints.Add(pointObj);
                }
            }
        }
        finally
        {
            // 确保进度条被清除
            EditorUtility.ClearProgressBar();
            isGenerating = false;
            
            // 刷新场景视图
            SceneView.RepaintAll();
        }
    }

    private Vector3 GetRandomPointInTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        
        float u = 1 - r1;
        float v = r1 * (1 - r2);
        float w = r1 * r2;
        
        return u * p1 + v * p2 + w * p3;
    }

    private void ClearGeneratedPoints()
    {
        foreach (GameObject point in generatedPoints)
        {
            if (point != null)
            {
                Undo.DestroyObjectImmediate(point);
            }
        }
        generatedPoints.Clear();
        
        // 重置点数计数
        foreach (var target in targetObjects)
        {
            target.pointCount = 0;
        }
        
        SceneView.RepaintAll();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += Repaint;
    }

    void OnDisable()
    {
        // 确保进度条被清除
        EditorUtility.ClearProgressBar();
        
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= Repaint;
    }
}