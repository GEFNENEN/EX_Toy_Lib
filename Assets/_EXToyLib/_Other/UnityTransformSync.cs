using System;
using EXToyLib;
using UnityEngine;

namespace _EXToyLib._Other
{
    [RequireComponent(typeof(SecondOrderDynamicsComponent))]
    public class UnityTransformSync : MonoBehaviour
    {
        public Transform target;
        private SecondOrderDynamicsComponent _dynamics;
        
        private void Awake()
        {
            _dynamics = GetComponent<SecondOrderDynamicsComponent>();
        }

        private void Update()
        {
            if (target == null) return;
            switch (_dynamics.ValueType)
            {
                case SecondOrderDynamicValueType.Position:
                    _dynamics.Dynamics.SetInput(target.position);
                    break;
                case SecondOrderDynamicValueType.Rotation:
                    _dynamics.Dynamics.SetInput(target.localEulerAngles);
                    break;
                case SecondOrderDynamicValueType.Scale:
                    _dynamics.Dynamics.SetInput(target.localScale);
                    break;
                case SecondOrderDynamicValueType.Custom:
                default:
                    break;
            }
        }
    }
}