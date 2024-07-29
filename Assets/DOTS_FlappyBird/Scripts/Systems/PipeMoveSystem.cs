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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

/*
 * Moves all Pipes, fires event when Pipe passes player (score)
 */
public class PipeMoveSystem : JobComponentSystem {

    public event EventHandler OnPipePassedPlayer;
    public struct OnPipePassedEvent : IComponentData { public int Value; }

    private DOTSEvents_NextFrame<OnPipePassedEvent> dotsEvents;

    protected override void OnCreate() {
        dotsEvents = new DOTSEvents_NextFrame<OnPipePassedEvent>(World);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;
        DOTSEvents_NextFrame<OnPipePassedEvent>.EventTrigger eventTrigger = dotsEvents.GetEventTrigger();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref Pipe pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (pipe.isBottom && xBefore > 0 && xAfter <= 0) {
                // Passed the player
                eventTrigger.TriggerEvent(entityInQueryIndex);
            }
        }).Schedule(inputDeps);
        dotsEvents.CaptureEvents(eventTrigger, jobHandle, (OnPipePassedEvent onPipePassedEvent) => {
            OnPipePassedPlayer?.Invoke(this, EventArgs.Empty);
        });

        return jobHandle;
    }

}