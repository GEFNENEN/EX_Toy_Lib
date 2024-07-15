using Unity.Entities;
using Unity.Mathematics;

public struct GraveProperty : IComponentData
{
    public float2 gravePosition;
    public float2 graveSize;
    public int graveID;
    public Entity graveStonePrefab;
}
