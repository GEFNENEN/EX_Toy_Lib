using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECS.System
{
    public partial struct SpawnSystem : ISystem
    {
        private uint updateCounter;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Spawner>();
            state.RequireForUpdate<UsePrefab>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spinningCubeQuery = SystemAPI.QueryBuilder().WithAll<RotationSpeed>().Build();

            if (spinningCubeQuery.IsEmpty)
            {
                var prefab = SystemAPI.GetSingleton<Spawner>().prefab;
                var instances = state.EntityManager.Instantiate(prefab,5,Allocator.Temp);
    
                foreach (var entity in instances)
                {
                    //var seed = Random.CreateFromIndex(updateCounter++);
                    var random = Random.CreateFromIndex(updateCounter++);
                    var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                    transform.ValueRW.Position = random.NextFloat3(-10,10)-new float3(1,-1 , 0.5f)*20;
                }
            }
            else
            {
                var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
                foreach (var (speed,entity) in SystemAPI.Query<RefRW<RotationSpeed>>().WithEntityAccess())
                {
                    speed.ValueRW.lifeTime -= Time.deltaTime;
                    if (!(speed.ValueRW.lifeTime <= 0)) continue;
                    ecb.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
        
        public void OnFixedUpdate(ref SystemState state)
        {
            OnUpdate(ref state);
        }
    }
}