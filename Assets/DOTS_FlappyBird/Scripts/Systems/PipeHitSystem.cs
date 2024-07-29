/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Collections;

/*
 * Tests for Trigger Collisions between Bird and Pipe or Wall
 */
public class PipeHitSystem : JobComponentSystem {

    public event EventHandler OnPipeHitPlayer;
    public struct OnPipeHitPlayerEvent : IComponentData {
        public int Value;
    }

    private struct PipeTrigger : ITriggerEventsJob {

        [ReadOnly] public ComponentLookup<Pipe> tagPipeComponentDataFromEntity;
        [ReadOnly] public ComponentLookup<Tag_Wall> tagWallComponentDataFromEntity;
        [ReadOnly] public ComponentLookup<Tag_Bird> tagBirdComponentDataFromEntity;
        public DOTSEvents_SameFrame<OnPipeHitPlayerEvent>.EventTrigger_NotConcurrent onPipeHitPlayerEventTrigger;

        public void Execute(TriggerEvent triggerEvent) {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            Entity birdEntity = Entity.Null;
            Entity pipeEntity = Entity.Null;
            Entity wallEntity = Entity.Null;

            if (tagBirdComponentDataFromEntity.HasComponent(entityA)) birdEntity = entityA;
            if (tagBirdComponentDataFromEntity.HasComponent(entityB)) birdEntity = entityB;

            if (tagPipeComponentDataFromEntity.HasComponent(entityA)) pipeEntity = entityA;
            if (tagPipeComponentDataFromEntity.HasComponent(entityB)) pipeEntity = entityB;

            if (tagWallComponentDataFromEntity.HasComponent(entityA)) wallEntity = entityA;
            if (tagWallComponentDataFromEntity.HasComponent(entityB)) wallEntity = entityB;

            if ((birdEntity != Entity.Null && pipeEntity != Entity.Null) ||
                (birdEntity != Entity.Null && wallEntity != Entity.Null)) {
                // Collision between Bird and Pipe or Bird and Wall
                onPipeHitPlayerEventTrigger.TriggerEvent();
            }
        }

    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    private DOTSEvents_SameFrame<OnPipeHitPlayerEvent> onPipeHitPlayerDOTSEvent;

    protected override void OnCreate() {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        onPipeHitPlayerDOTSEvent = new DOTSEvents_SameFrame<OnPipeHitPlayerEvent>(World);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        DOTSEvents_SameFrame<OnPipeHitPlayerEvent>.EventTrigger_NotConcurrent onPipeHitPlayerEventTrigger = onPipeHitPlayerDOTSEvent.GetEventTriggerNotConcurrent();

        JobHandle jobHandle = new PipeTrigger {
            tagBirdComponentDataFromEntity = GetComponentDataFromEntity<Tag_Bird>(),
            tagPipeComponentDataFromEntity = GetComponentDataFromEntity<Pipe>(),
            tagWallComponentDataFromEntity = GetComponentDataFromEntity<Tag_Wall>(),
            onPipeHitPlayerEventTrigger = onPipeHitPlayerEventTrigger,
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        onPipeHitPlayerDOTSEvent.CaptureEvents(onPipeHitPlayerEventTrigger, jobHandle, (OnPipeHitPlayerEvent onPipeHitPlayerEvent) => {
            OnPipeHitPlayer?.Invoke(this, EventArgs.Empty);
        });

        return jobHandle;
    }

}
