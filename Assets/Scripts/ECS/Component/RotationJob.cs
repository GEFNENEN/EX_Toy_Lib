using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DefaultNamespace.ECS.Component
{
    [BurstCompile]
    public partial struct RotationJob : IJobEntity
    {
        public float deltaTime;

        private void Execute(ref LocalTransform localTransform, in RotationSpeed rotationSpeed)
        {
            localTransform = localTransform.RotateY(deltaTime * rotationSpeed.speed);
        }
    }
}