using DefaultNamespace.GAS_ECS.Tag.Component;
using Unity.Burst;
using Unity.Entities;

namespace DefaultNamespace.GAS_ECS.Tag
{
    public partial struct CheckTagSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var tag in SystemAPI.Query<RefRO<GameplayTagComponent>>())
            {
                var tagEnum = tag.ValueRO.TagEnum;
                var tagFight = GTagLib.Fight;
                tagFight.HasTag(tagEnum);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}