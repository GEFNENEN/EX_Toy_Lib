using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

public class DOTS_GameHandler : JobComponentSystem {

    public static DOTS_GameHandler Instance { get; private set; }

    public event EventHandler OnGameStarted;
    public event EventHandler OnGameOver;
     
    protected override void OnCreate() {
        Instance = this;
        Loader.OnSceneUnLoaded += Loader_OnSceneUnLoaded;
        World.GetOrCreateSystem<PipeHitSystem>().OnPipeHitPlayer += DOTS_GameHandler_OnPipeHitPlayer;
        World.GetOrCreateSystem<BirdInputSystem>().OnBirdJump += DOTS_GameHandler_OnBirdJump;
        SetSystemsEnabled(false);
    }

    private void Loader_OnSceneUnLoaded(object sender, EventArgs e) {
        SetSystemsEnabled(false);
        // Clean up all Entities
        EntityManager.DestroyEntity(EntityManager.UniversalQuery);
    }

    private void DOTS_GameHandler_OnBirdJump(object sender, System.EventArgs e) {
        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();
            if (gameState.state == GameState.State.WaitingToStart) {
                gameState.state = GameState.State.Playing;
                SetSingleton(gameState);

                SetSystemsEnabled(true);
                World.GetOrCreateSystem<PipeSpawnerSystem>().Reset();

                OnGameStarted?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void DOTS_GameHandler_OnPipeHitPlayer(object sender, System.EventArgs e) {
        SetSystemsEnabled(false);

        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();
            gameState.state = GameState.State.Dead;
            SetSingleton(gameState);
        }

        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    private void SetSystemsEnabled(bool enabled) {
        World.GetOrCreateSystem<PipeHitSystem>().Enabled = enabled;
        World.GetOrCreateSystem<PipeMoveSystem>().Enabled = enabled;
        World.GetOrCreateSystem<PipeDestroySystem>().Enabled = enabled;
        World.GetOrCreateSystem<PipeSpawnerSystem>().Enabled = enabled;
        World.GetOrCreateSystem<BirdControlSystem>().Enabled = enabled;
        World.GetOrCreateSystem<GroundMoveSystem>().Enabled = enabled;
        World.GetOrCreateSystem<CloudMoveSystem>().Enabled = enabled;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        return default;
    }

}
