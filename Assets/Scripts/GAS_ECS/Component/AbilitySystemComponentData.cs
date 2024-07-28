using Unity.Entities;

namespace DefaultNamespace.GAS_ECS.Component
{
    public struct AbilitySystemComponentData : IComponentData
    {
        public int level;
        public int gameplayEffectContainerEntityId;
        public int abilityContainerEntityId;
    }
}