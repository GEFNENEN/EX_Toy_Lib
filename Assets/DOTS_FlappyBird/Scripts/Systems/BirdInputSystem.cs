using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;

public class BirdInputSystem : JobComponentSystem {

    public event EventHandler OnBirdJump;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        bool jumpInputDown = Input.GetKeyDown(KeyCode.Space);

        if (jumpInputDown) {
            OnBirdJump?.Invoke(this, EventArgs.Empty);
        }

        return Entities.WithAll<Tag_Bird>().ForEach((ref MoveSpeed moveSpeed) => {
            if (jumpInputDown) {
                moveSpeed.moveDirSpeed.y = 8f;
            }
        }).Schedule(inputDeps);
    }
}
