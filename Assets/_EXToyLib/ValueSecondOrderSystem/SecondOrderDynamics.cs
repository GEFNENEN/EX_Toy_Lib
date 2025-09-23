using System;
using UnityEngine;

/// <summary>
///     二阶动力学平滑算法，用于平滑输入信号（如位置、旋转），避免突变。
///     原理：通过模拟二阶线性系统（质量-弹簧-阻尼），实现自然的跟随效果。
/// </summary>
public class SecondOrderDynamics
{
    /// <summary> 阻尼系数（由阻尼比z和频率f计算） </summary>
    private readonly float _k1;

    /// <summary> 刚度系数（由频率f计算） </summary>
    private readonly float _k2;

    /// <summary> 缩放因子（由缩放r、阻尼比z和频率f计算） </summary>
    private readonly float _k3;

    /// <summary> 上一次的输入值（用于估计速度） </summary>
    private Vector3 _previousInput;

    /// <summary> 当前状态值（输出）：位置/旋转 </summary>
    private Vector3 _statePosition;

    /// <summary> 当前状态值（输出）：速度 </summary>
    private Vector3 _stateVelocity;

    #region 构造函数

    /// <summary>
    ///     初始化二阶动力学系统。
    /// </summary>
    /// <param name="frequency"> 系统频率（Hz）：越高，响应速度越快（推荐5-10Hz）。 </param>
    /// <param name="damping"> 阻尼比（0-1）：0=无阻尼（持续震荡），0.5=欠阻尼（轻微超调），1=临界阻尼（无超调）。 </param>
    /// <param name="scale"> 缩放因子（1-∞）：越大，超调量越大（推荐1）。 </param>
    /// <param name="initialValue"> 初始状态值（如物体初始位置）。 </param>
    public SecondOrderDynamics(float frequency, float damping, float scale, Vector3 initialValue)
    {
        // 校验参数合法性（避免除以零）
        if (frequency <= 0)
            throw new ArgumentException("频率必须大于0！", nameof(frequency));
        if (damping < 0)
            throw new ArgumentException("阻尼比不能小于0！", nameof(damping));

        // 计算动力学常数（核心公式）
        var pi = Mathf.PI;
        _k1 = damping / (pi * frequency);
        _k2 = 1 / (2 * pi * frequency * (2 * pi * frequency));
        _k3 = scale * damping / (2 * pi * frequency);

        // 初始化状态（初始值等于输入值，速度为0）
        _previousInput = initialValue;
        _statePosition = initialValue;
        _stateVelocity = Vector3.zero;
    }

    #endregion

    /// <summary>
    ///     更新动力学状态，返回平滑后的值。
    /// </summary>
    /// <param name="deltaTime"> 时间步长（推荐使用Time.deltaTime）。 </param>
    /// <param name="currentInput"> 当前输入值（如目标位置）。 </param>
    /// <param name="inputVelocity"> 输入值的导数（如目标速度，可选）。 </param>
    /// <returns> 平滑后的状态值（如平滑后的位置）。 </returns>
    public Vector3 Update(float deltaTime, Vector3 currentInput, Vector3? inputVelocity = null)
    {
        if (inputVelocity == null)
        {
            // 如果未提供输入速度，用当前输入与上一次输入的差值估计速度（一阶差分）
            inputVelocity = (currentInput - _previousInput) / deltaTime;
            // 更新上一次输入（用于下一次速度估计）
            _previousInput = currentInput;
        }

        // 欧拉积分：更新状态位置（位置 = 位置 + 速度 * 时间步长）
        _statePosition += _stateVelocity * deltaTime;

        // 欧拉积分：更新状态速度（速度 = 速度 + 加速度 * 时间步长）
        // 加速度公式：(输入 + 缩放因子*输入速度 - 当前位置 - 阻尼系数*当前速度) / 刚度系数
        _stateVelocity += deltaTime *
            (currentInput + _k3 * inputVelocity.Value - _statePosition - _k1 * _stateVelocity) / _k2;

        // 返回平滑后的位置
        return _statePosition;
    }

    public void Reset(Vector3 zero)
    {
        _previousInput = zero;
        _statePosition = zero;
        _stateVelocity = zero;
    }
}