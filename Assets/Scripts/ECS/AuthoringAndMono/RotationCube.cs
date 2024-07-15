using DefaultNamespace.ECS.Component;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AuthoringAndMono
{
    public class RotationCube : MonoBehaviour
    {
        public float speed = 10f;
        private class RotationDubeBaker : Baker<RotationCube>
        {
            public override void Bake(RotationCube authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RotationSpeed()
                {
                    speed = math.radians(authoring.speed),
                    lifeTime = 2f
                });
            }
        }
    }
}