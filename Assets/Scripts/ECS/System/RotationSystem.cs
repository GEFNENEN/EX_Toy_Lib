// using DefaultNamespace.ECS.Component;
// using ECS.AuthoringAndMono;
// using Unity.Burst;
// using Unity.Entities;
// using Unity.Transforms;
// using UnityEngine;
// using UnityEngine.PlayerLoop;
//
// namespace ECS.System
// {
//     public partial struct RotationSystem : ISystem
//     {
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//             state.RequireForUpdate<MainThread>();
//         }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             var deltaTime = Time.deltaTime;
//             foreach (var (transform,speed) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<RotationSpeed>>())
//             {
//                 transform.ValueRW = transform.ValueRO.RotateY(deltaTime * speed.ValueRO.speed);
//             }
//         }
//
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state)
//         {
//
//         }
//     }
// }


using DefaultNamespace.ECS.Component;
using ECS.AuthoringAndMono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HelloCube.GameObjectSync
{
#if !UNITY_DISABLE_MANAGED_COMPONENTS
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DirectoryManaged>();
            state.RequireForUpdate<ExecuteGameObjectSync>();
        }

        // This OnUpdate accesses managed objects, so it cannot be burst compiled.
        public void OnUpdate(ref SystemState state)
        {
            var directory = SystemAPI.ManagedAPI.GetSingleton<DirectoryManaged>();
            if (!directory.RotationToggle.isOn)
            {
                return;
            }

            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, speed, go) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>, RotatorGO>())
            {
                transform.ValueRW = transform.ValueRO.RotateY(
                    speed.ValueRO.speed * deltaTime);

                // Update the associated GameObject's transform to match.
                go.Value.transform.rotation = transform.ValueRO.Rotation;
            }
        }
    }
#endif
}