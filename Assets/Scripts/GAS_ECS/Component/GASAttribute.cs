using Unity.Entities;

namespace DefaultNamespace.GAS_ECS.Component
{
    public struct GASAttribute : IComponentData
    {
        public float baseValue;
        public float currentValue;
    }
}