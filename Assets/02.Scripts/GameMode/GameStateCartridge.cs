using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameStateCartridge : GameModeStateBase
{
    public override void Enter()
    {
        Test();
        Debug.Log("cartiridge enter");
    }
    
    private void Test()
    {
        // 테스트 용
        TestAsync();
    }

    private async UniTaskVoid TestAsync()
    {
        await UniTask.WaitForSeconds(1f);
        
        _gameMode.RequestStateChange(EModeState.Spawn);
    }
    public override void Tick() { }
    public override void Exit() { }
}
