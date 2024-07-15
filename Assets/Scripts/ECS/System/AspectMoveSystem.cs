using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ECS.System
{
    public partial struct AspectMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UseAspect>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = Time.deltaTime;
            foreach (var (transform,speed) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<RotationSpeed>>())
            {
                transform.ValueRW = transform.ValueRO.RotateY(deltaTime * speed.ValueRO.speed);
            }

            var elapsedTime = SystemAPI.Time.ElapsedTime;
            foreach (var moveAspect in SystemAPI.Query<MoveAspect>())
            {
                moveAspect.Move((float)elapsedTime);
            }
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}