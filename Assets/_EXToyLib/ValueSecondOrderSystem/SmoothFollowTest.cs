// SmoothFollow.cs（挂在跟随物体上）
using UnityEngine;

[System.Serializable]
public class SmoothFollow : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target; // 目标物体

    [Header("动力学参数（曲线预览会实时更新）")]
    [Tooltip("频率（Hz）：越高响应越快（推荐5-10）")]
    [Range(0.1f, 6f)]
    public float frequency = 5f;
    
    [Tooltip("阻尼比：0=无阻尼（震荡），1=临界阻尼（无超调）")]
    [Range(0f, 1f)]
    public float damping = 0.5f;
    
    [Tooltip("缩放因子：越大超调量越大（推荐1）")]
    [Range(-10f, 10f)]
    public float scale = 1f;

    private SecondOrderDynamics _dynamics; // 二阶动力学实例

    void Start()
    {
        _dynamics = new SecondOrderDynamics(frequency, damping, scale, transform.position);
    }

    void Update()
    {
        if (target == null) return;
        Vector3 targetPos = target.position;
        Vector3 smoothedPos = _dynamics.Update(Time.deltaTime, targetPos);
        transform.position = smoothedPos;
    }
}