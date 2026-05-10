public class GameStateOver : GameModeStateBase
{
    public override void Enter()
    {
        _gameMode.GameOver();
    }
}
