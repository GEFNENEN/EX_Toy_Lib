using UnityEngine;

namespace EXToyLib
{
    [System.Serializable]
    public class SecondOrderDynamicsComponent : MonoBehaviour
    {
        [HideInInspector]
        public bool avator = true; // 是否使用替身
        
        [HideInInspector]
        public Transform target; // 目标物体

        [HideInInspector]
        public bool autoUpdate;
        
        [HideInInspector]
        public SecondOrderDynamicValueType ValueType = SecondOrderDynamicValueType.Position;

        public float F => _dynamics.F;
        public float Z => _dynamics.Z;
        public float R => _dynamics.R;

        private SecondOrderDynamics _dynamics = new SecondOrderDynamics(); // 二阶动力学实例
        public SecondOrderDynamics Dynamics => _dynamics; // 二阶动力学实例

        void Update()
        {
            if (autoUpdate) _dynamics.UpdateFactors();

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