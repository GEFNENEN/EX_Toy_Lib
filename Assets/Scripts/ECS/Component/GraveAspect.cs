using Unity.Entities;
using Unity.Transforms;

namespace DefaultNamespace.ECS.Component
{
    public readonly partial struct GraveAspect : IAspect
    {
        public readonly Entity Entity;
        
        private readonly RefRO<LocalTransform> _localTransform;
        
        private readonly RefRO<GraveProperty> _graveProperty;
        private readonly RefRW<GraveRandom> _graveRandom;
    }
}