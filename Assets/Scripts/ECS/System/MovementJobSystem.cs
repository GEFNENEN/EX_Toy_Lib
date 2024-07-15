using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.System
{
    public partial struct MovementJobSystem : ISystem
    {
        private float currentTime;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IsJobEntity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            currentTime += Time.deltaTime;
            var job = new MovementJob(){speed = 1,magnitude = 2,time = currentTime};
            job.Schedule();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    public partial struct MovementJob : IJobEntity
    {
        public float time;
        public float speed;
        public float magnitude;
        
        void Execute(ref LocalTransform localTransform,in RotationSpeed rotationSpeed)
        {
            localTransform.Position.y = magnitude * math.sin(time * speed);
        }
    }
}