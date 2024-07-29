using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class CloudMoveSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1, 0, 0);
        float moveSpeed = 2.5f;

        return Entities.WithAll<Tag_Cloud>().ForEach((ref Translation translation) => {
            translation.Value += moveDir * moveSpeed * deltaTime;

            float translationEdge = 36.5f;
            if (translation.Value.x <= -translationEdge) {
                translation.Value += new float3(translationEdge, 0, 0);
            }
        }).Schedule(inputDeps);
    }

}