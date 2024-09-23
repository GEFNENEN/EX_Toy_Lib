using Unity.Entities;

namespace DefaultNamespace.GAS_ECS.Component
{
    public struct GASAttributeSet : IComponentData
    {
        public GASAttribute hp;
        public GASAttribute mp;
    }
}