using UnityEngine;
using Sirenix.OdinInspector; // 添加Odin命名空间

namespace EXToyLib
{
    [System.Serializable]
    public class SecondOrderDynamicsComponent : MonoBehaviour
    {
        [BoxGroup("Settings")]
        [LabelText("使用替身")]
        public bool avator = true; // 是否使用替身
        
        [BoxGroup("Settings")]
        [ShowIf("avator")]
        [LabelText("替身目标")]
        public Transform target; // 目标物体

        [BoxGroup("Settings")]
        [LabelText("自动更新参数")]
        public bool autoUpdate;
        
        [BoxGroup("Settings")]
        [EnumToggleButtons]
        [LabelText("影响属性")]
        public SecondOrderDynamicValueType ValueType = SecondOrderDynamicValueType.Position;

        [BoxGroup("Parameters")]
        [MinValue(0.1f), MaxValue(7f)]
        [OnValueChanged("UpdateDynamicsFactors")]
        public float Frequency = 1f;

        [BoxGroup("Parameters")]
        [MinValue(0f), MaxValue(1f)]
        [OnValueChanged("UpdateDynamicsFactors")]
        public float Damping = 1f;

        [BoxGroup("Parameters")]
        [MinValue(-10f), MaxValue(10f)]
        [OnValueChanged("UpdateDynamicsFactors")]
        public float Scale = 0f;

        private SecondOrderDynamics _dynamics = new SecondOrderDynamics(); // 二阶动力学实例
        public SecondOrderDynamics Dynamics => _dynamics; // 二阶动力学实例

        private void Start()
        {
            UpdateDynamicsFactors();
        }
        
        private void UpdateDynamicsFactors()
        {
            _dynamics.SetF(Frequency);
            _dynamics.SetZ(Damping);
            _dynamics.SetR(Scale);
            _dynamics.UpdateFactors();
        }

        void Update()
        {
            if (autoUpdate) UpdateDynamicsFactors();

            if (avator)
            {
                if (target != null)
                    switch (ValueType)
                    {
                        case SecondOrderDynamicValueType.Position:
                            transform.position = _dynamics.Update(Time.deltaTime, target.position);
                            break;
                        case SecondOrderDynamicValueType.Rotation:
                            transform.localEulerAngles = _dynamics.Update(Time.deltaTime, target.localEulerAngles);
                            break;
                        case SecondOrderDynamicValueType.Scale:
                            transform.localScale = _dynamics.Update(Time.deltaTime, target.localScale);
                            break;
                        case SecondOrderDynamicValueType.Custom:
                        default:
                            break;
                    }
            }
            else
                switch (ValueType)
                {
                    case SecondOrderDynamicValueType.Position:
                        transform.position = _dynamics.Update(Time.deltaTime);
                        break;
                    case SecondOrderDynamicValueType.Rotation:
                        transform.localEulerAngles = _dynamics.Update(Time.deltaTime);
                        break;
                    case SecondOrderDynamicValueType.Scale:
                        transform.localScale = _dynamics.Update(Time.deltaTime);
                        break;
                    case SecondOrderDynamicValueType.Custom:
                    default:
                        break;
                }
        }
    }
}