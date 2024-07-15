using DefaultNamespace.ECS.Component;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECS.AuthoringAndMono
{
    public class GraveMono : MonoBehaviour
    {
        public float2 gravePosition;
        public float2 graveSize;
        public int graveID;
        public GameObject graveStonePrefab;
        public uint RandomSeed;
    }

    public class GraveBaker : Baker<GraveMono>
    {
        public override void Bake(GraveMono authoring)
        {
            AddComponent(new GraveProperty()
            {
                gravePosition = authoring.gravePosition,
                graveSize = authoring.graveSize,
                graveID = authoring.graveID,
                graveStonePrefab = GetEntity(authoring.graveStonePrefab,TransformUsageFlags.Dynamic),
            });
            
            AddComponent(new GraveRandom()
            {
                Value = Random.CreateFromIndex(authoring.RandomSeed),
            });
        }
    }
}