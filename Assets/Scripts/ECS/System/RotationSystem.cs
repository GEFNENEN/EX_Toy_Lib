using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ECS.System
{
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainThread>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = Time.deltaTime;
            foreach (var (transform,speed) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<RotationSpeed>>())
            {
                transform.ValueRW = transform.ValueRO.RotateY(deltaTime * speed.ValueRO.speed);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}