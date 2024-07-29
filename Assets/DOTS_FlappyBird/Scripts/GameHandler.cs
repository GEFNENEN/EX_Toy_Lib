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

public class GameHandler : MonoBehaviour {

    public static GameHandler Instance { get; private set; }

    public event EventHandler OnGameStarted;
    public event EventHandler OnGameOver;

    private DOTS_GameHandler dotsGameHandler;

    private void Awake() {
        Instance = this;
        new ScoreHandler();
    }

    private void Start() {
        ScoreHandler.Instance.OnScoreChanged += Instance_OnScoreChanged;
        dotsGameHandler = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DOTS_GameHandler>();
        dotsGameHandler.OnGameOver += DotsGameHandler_OnGameOver;
        dotsGameHandler.OnGameStarted += DotsGameHandler_OnGameStarted;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BirdInputSystem>().OnBirdJump += GameHandler_OnBirdJump;
    }

    private void OnDestroy() {
        ScoreHandler.Instance.OnScoreChanged -= Instance_OnScoreChanged;
        ScoreHandler.Instance.DestroySelf();
        dotsGameHandler.OnGameOver -= DotsGameHandler_OnGameOver;
        dotsGameHandler.OnGameStarted -= DotsGameHandler_OnGameStarted;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BirdInputSystem>().OnBirdJump -= GameHandler_OnBirdJump;
    }

    private void GameHandler_OnBirdJump(object sender, EventArgs e) {
        SoundManager.PlaySound(SoundManager.Sound.BirdJump);
    }

    private void DotsGameHandler_OnGameStarted(object sender, System.EventArgs e) {
        Debug.Log("Game Started!");
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    private void DotsGameHandler_OnGameOver(object sender, System.EventArgs e) {
        Debug.Log("Game Over!");
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    private void Instance_OnScoreChanged(object sender, ScoreHandler.OnScoreChangedEventArgs e) {
        Debug.Log("Score: " + e.score);
        SoundManager.PlaySound(SoundManager.Sound.Score);
    }

}
