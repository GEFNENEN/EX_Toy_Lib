// SmoothFollow.cs（挂在跟随物体上）
using UnityEngine;

[System.Serializable]
public class SecondOrderDynamicsComponent : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target; // 目标物体
    
    private float _frequency = 5f;
    private float _damping = 0.5f;
    private float _scale = 1f;
    
    public float F => _frequency;
    public float Z => _damping;
    public float R => _scale;
    
    public void SetF(float f) => _frequency = f;
    public void SetZ(float z) => _damping = z;
    public void SetR(float r) => _scale = r;

    private SecondOrderDynamics _dynamics; // 二阶动力学实例

    void OnEnable()
    {
        _dynamics = new SecondOrderDynamics(_frequency, _damping, _scale,transform.position);
    }

    void Update()
    {
        if (target == null) return;
        var targetPos = target.position;
        var smoothedPos = _dynamics.Update(Time.deltaTime, targetPos);
        transform.position = smoothedPos;
    }
}