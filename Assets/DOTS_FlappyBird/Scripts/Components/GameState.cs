using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameState : IComponentData {

    public enum State {
        WaitingToStart,
        Playing,
        Dead
    }

    public State state;

}