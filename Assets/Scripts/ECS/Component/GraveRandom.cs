using Unity.Entities;
using Unity.Mathematics;

namespace DefaultNamespace.ECS.Component
{
    public struct GraveRandom : IComponentData
    {
        public Random Value;
    }
}