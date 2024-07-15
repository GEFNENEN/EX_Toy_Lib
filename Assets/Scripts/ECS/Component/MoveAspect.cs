using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DefaultNamespace.ECS.Component
{
    public readonly partial struct MoveAspect : IAspect
    {
        readonly RefRW<LocalTransform> _localTransform;
        readonly RefRO<RotationSpeed> _rotationSpeed;
        
        public void Move(double escapeTime)
        {
            _localTransform.ValueRW.Position.x = 2 * math.sin((float)(escapeTime * _rotationSpeed.ValueRO.speed));
        }
    }
}