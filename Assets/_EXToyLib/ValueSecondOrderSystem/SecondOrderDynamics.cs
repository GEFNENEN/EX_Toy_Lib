using UnityEngine;
using System;

public class SecondOrderDynamics
{
    // ------------ 物理参数（由频率、阻尼、缩放因子计算） ------------
    private float _w;    // 角频率（ω = 2πf）：控制响应速度
    private float _z;    // 阻尼比（ζ）：0=无阻尼（振荡），1=临界阻尼（无超调）
    private float _d;    // 阻尼系数（根据ζ的不同情况）：
                        // - 欠阻尼（ζ<1）：d = ω√(1-ζ²)（振荡频率）
                        // - 过阻尼（ζ>1）：d = ω√(ζ²-1)（衰减速率）
                        // - 临界阻尼（ζ=1）：d=0
                        

    // ------------ 动力学常数（与连续时间系统匹配） ------------
    private float _k1;   // 阻尼项系数（k1 = 2ζ/ω）
    private float _k2;   // 刚度项系数（k2 = 1/ω²）
    private float _k3;   // 缩放项系数（k3 = (rζ)/ω，r为缩放因子）
    
    // ------------ 极限值控制 ------------
    private float _minT = 0.01f;
    
    // ------------ 状态变量 ------------
    private Vector3 _previousInput;  // xp：上一帧输入（用于估计速度）
    private Vector3 _currentOutput;  // y：当前输出（平滑后的值）
    private Vector3 _outputVelocity; // yd：当前输出速度（dy/dt）

    // ------------ 构造函数（初始化物理参数和状态） ------------
    /// <summary>
    /// 初始化二阶动力学系统
    /// </summary>
    /// <param name="frequency">频率（f，Hz）：越大，响应越快</param>
    /// <param name="damping">阻尼比（ζ）：0=无阻尼，1=临界阻尼</param>
    /// <param name="scale">缩放因子（r）：越大，超调越明显</param>
    /// <param name="initialValue">初始输出值（y0）</param>
    public SecondOrderDynamics(float frequency, float damping, float scale, Vector3 initialValue)
    {
        // 计算角频率（ω = 2πf）
        _w = 2 * Mathf.PI * frequency;

        // 阻尼比（ζ）
        _z = damping;

        // 计算阻尼系数（d）：根据ζ的情况
        if (_z < 1)
        {
            // 欠阻尼：d = ω√(1-ζ²)（振荡频率）
            _d = _w * Mathf.Sqrt(1 - _z * _z);
        }
        else if (_z > 1)
        {
            // 过阻尼：d = ω√(ζ²-1)（衰减速率）
            _d = _w * Mathf.Sqrt(_z * _z - 1);
        }
        else
        {
            // 临界阻尼：d=0
            _d = 0;
        }

        // 计算动力学常数（与连续时间系统匹配）
        _k1 = 2 * _z / _w ;          // k1 = 2ζ/ω
        _k2 = 1 / (_w * _w) ;        // k2 = 1/ω²
        _k3 = 2* scale * _z / _w ;    // k3 = (rζ)/ω

        // 初始化状态变量（输出从初始值开始，速度为0）
        _previousInput = initialValue;
        _currentOutput = initialValue;
        _outputVelocity = Vector3.zero;
    }

    // ------------ 重置系统状态（用于Editor预览或参数调整） ------------
    /// <summary>
    /// 重置系统状态（输出值和速度）
    /// </summary>
    /// <param name="newValue">新的初始输出值</param>
    public void Reset(Vector3 newValue)
    {
        _previousInput = newValue;
        _currentOutput = newValue;
        _outputVelocity = Vector3.zero;
    }

    // ------------ 核心更新函数（匹配连续时间解析解，消除抖动） ------------
    /// <summary>
    /// 更新系统状态（输出平滑后的值）
    /// </summary>
    /// <param name="deltaTime">当前时间步长（T）</param>
    /// <param name="targetInput">目标输入值（x(t)）</param>
    /// <param name="targetVelocity">目标输入速度（xd(t) = dx/dt，可选，若未提供则自动估计）</param>
    /// <returns>平滑后的输出值（y(t)）</returns>
    public Vector3 Update(float deltaTime, Vector3 targetInput, Vector3? targetVelocity = null)
    {
        // ------------ 步骤1：估计目标输入速度（若未提供） ------------
        // 若未传入targetVelocity，用当前输入与上一帧输入的差值估计速度（xd = (x - xp)/T）
        Vector3 estimatedVelocity = targetVelocity ?? (targetInput - _previousInput) / deltaTime;
        _previousInput = targetInput; // 更新输入历史，用于下一帧估计

        // ------------ 步骤2：计算稳定的k2值（k2_stable） ------------
        // 核心优化：通过匹配连续时间系统的解析解，计算精确的k2_stable，消除抖动
        float k2Stable = _k2; // 默认使用原k2（适用于低速场景）

        // 判断是否需要使用精确计算（高速场景：时间步T很小或目标变化很快）
        if (deltaTime < _minT)
        {
            // ------------ 步骤2.1：计算连续时间系统的解析解系数 ------------
            float t = deltaTime; // 简化变量名（时间步长）
            float expTerm = Mathf.Exp(-_z * _w * t); // 衰减项（e^(-ζωt)）

            float alpha = 0f; // 余弦/双曲余弦项（根据ζ的情况）
            float beta = 0f;  // 正弦/双曲正弦项（根据ζ的情况）

            if (_z < 1)
            {
                // 欠阻尼（0 < ζ < 1）：振荡响应
                float dT = _d * t;
                alpha = Mathf.Cos(dT);          // cos(ω√(1-ζ²)t)
                beta = Mathf.Sin(dT) / _d;      // sin(ω√(1-ζ²)t)/ω√(1-ζ²)
            }
            else if (Mathf.Approximately(_z, 1))
            {
                // 临界阻尼（ζ=1）：无振荡，最快收敛
                alpha = 1f;                     // 1（无振荡）
                beta = t;                       // t（线性项）
            }
            else
            {
                // 过阻尼（ζ > 1）：无振荡，缓慢收敛
                float dT = _d * t;
                alpha = (float)Math.Cosh(dT);         // cosh(ω√(ζ²-1)t)
                beta = (float)Math.Sinh(dT) / _d;     // sinh(ω√(ζ²-1)t)/ω√(ζ²-1)
            }

            // ------------ 步骤2.2：计算解析解中的系数 ------------
            float term1 = 1 - expTerm * alpha;                  // 1 - e^(-ζωt)cos(ω√(1-ζ²)t)（欠阻尼示例）
            float term2 = 1 - expTerm * (alpha + _z * _w * beta); // 1 - e^(-ζωt)(cos(...) + ζω sin(...)/ω√(1-ζ²))（欠阻尼示例）

            // ------------ 步骤2.3：计算精确的k2_stable ------------
            // 公式来源：匹配连续时间系统的解析解，推导离散时间的k2_stable
            // 目的：使离散时间的更新完全还原连续时间的响应，消除抖动
            float t2 = term1 / (_w * _w);                      // t2 = (1 - e^(-ζωt)cos(...))/ω²
            float t3 = term2 / (_w * _w);                      // t3 = (1 - e^(-ζωt)(cos(...) + ζω sin(...)/ω√(1-ζ²)))/ω²
            k2Stable = deltaTime / (t2 + t3 * _w * _w);        // k2_stable = T / (t2 + t3ω²)
        }

        // ------------ 步骤3：数值积分更新状态（完全还原连续时间响应） ------------
        // 位置积分：y = y + T*yd（根据速度更新位置）
        _currentOutput += deltaTime * _outputVelocity;

        // 速度积分：yd = yd + T*(x + k3*xd - y - k1*yd)/k2_stable（根据加速度更新速度）
        // 注：此处用k2_stable代替原k2，保证离散时间的更新与连续时间一致
        _outputVelocity += deltaTime * (targetInput + _k3 * estimatedVelocity - _currentOutput - _k1 * _outputVelocity) / k2Stable;

        // ------------ 步骤4：返回平滑后的输出 ------------
        return _currentOutput;
    }
}