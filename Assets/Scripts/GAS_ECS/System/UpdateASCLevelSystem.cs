using DefaultNamespace.GAS_ECS.Component;
using Unity.Burst;
using Unity.Entities;

namespace DefaultNamespace.GAS_ECS.System
{
    public partial struct UpdateASCLevelSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var asc in SystemAPI.Query<RefRW<AbilitySystemComponentData>>())
            {
                asc.ValueRW.level = 5;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}