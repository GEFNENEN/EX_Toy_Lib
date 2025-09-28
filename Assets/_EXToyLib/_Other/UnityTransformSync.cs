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
            foreach (var inst in _dynamics.instances)
            {
                switch (inst.ValueType)
                {
                    case SecondOrderDynamicValueType.Position:
                        inst.Dynamics.SetInput(target.position);
                        break;
                    case SecondOrderDynamicValueType.Rotation:
                        inst.Dynamics.SetInput(target.localEulerAngles);
                        break;
                    case SecondOrderDynamicValueType.Scale:
                        inst.Dynamics.SetInput(target.localScale);
                        break;
                    case SecondOrderDynamicValueType.Custom:
                    default:
                        break;
                }
            }
        }
    }
}