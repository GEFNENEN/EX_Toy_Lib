using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ScoreHandler {

    public static ScoreHandler Instance { get; private set; }

    public event EventHandler<OnScoreChangedEventArgs> OnScoreChanged;
    public class OnScoreChangedEventArgs : EventArgs {
        public int score;
    }

    private int score;

    public ScoreHandler() {
        //ResetHighscore();
        Instance = this;
        score = 0;

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PipeMoveSystem>().OnPipePassedPlayer += ScoreHandler_OnPipePassedPlayer;
        GameHandler.Instance.OnGameOver += GameHandler_OnGameOver;
    }

    private void GameHandler_OnGameOver(object sender, EventArgs e) {
        TrySetNewHighscore(score);
    }

    public void DestroySelf() {
        if (World.DefaultGameObjectInjectionWorld != null) {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PipeMoveSystem>().OnPipePassedPlayer -= ScoreHandler_OnPipePassedPlayer;
        }
        GameHandler.Instance.OnGameOver -= GameHandler_OnGameOver;
    }

    private void ScoreHandler_OnPipePassedPlayer(object sender, EventArgs e) {
        AddScore();
    }

    public int GetScore() {
        return score;
    }

    private void AddScore() {
        score++;
        OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs { score = score });
    }


    public int GetHighscore() {
        return PlayerPrefs.GetInt("highscore");
    }

    public bool TrySetNewHighscore(int score) {
        int currentHighscore = GetHighscore();
        if (score > currentHighscore) {
            // New Highscore
            PlayerPrefs.SetInt("highscore", score);
            PlayerPrefs.Save();
            return true;
        } else {
            return false;
        }
    }

    public void ResetHighscore() {
        PlayerPrefs.SetInt("highscore", 0);
        PlayerPrefs.Save();
    }

}
