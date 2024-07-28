using DefaultNamespace.GAS_ECS.Component;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.GAS_ECS.AuthoringAndMono
{
    public class AbilitySystemComponentAuthoring : MonoBehaviour
    {
        public int level;
        private class AbilitySystemComponentAuthoringBaker : Baker<AbilitySystemComponentAuthoring>
        {
            public override void Bake(AbilitySystemComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AbilitySystemComponentData { level = authoring.level });
            }
        }
    }
}