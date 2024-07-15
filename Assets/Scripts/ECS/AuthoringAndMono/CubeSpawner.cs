using Unity.Entities;
using UnityEngine;

namespace ECS.AuthoringAndMono
{
    public class CubeSpawner : MonoBehaviour
    {
        public GameObject prefab;

        private class CubeSpawnerBaker : Baker<CubeSpawner>
        {
            public override void Bake(CubeSpawner authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Spawner { prefab = GetEntity(authoring.prefab, TransformUsageFlags.None) });
            }
        }
    }
    
    public struct Spawner : IComponentData
    {
        public Entity prefab;
    }
}