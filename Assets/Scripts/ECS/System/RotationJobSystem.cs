using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace ECS.System
{
    public partial struct RotationJobSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IsJobEntity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RotationJob(){deltaTime = Time.deltaTime};
            job.Schedule();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}