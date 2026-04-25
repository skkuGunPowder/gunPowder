using Cysharp.Threading.Tasks;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameStateCartridge : GameModeStateBase
{
    public override void Enter()
    {
        EventManager.Instance.OnScreenClick += ProductionStart;
        EventManager.Instance.CartridgeStateEnter();
    }
    
    private void Test()
    {
        
    }
    private void ProductionStart()  // OnScreenClick 구독 함수
    {
        if (_gameMode is BattleMode battleMode)
        {
            if (battleMode.DeathOrderQueue.Count > 0)
            {
                PhotonPlayer player = battleMode.DeathOrderQueue.Dequeue();
                EventManager.Instance.CartridgeStart(player);
            }
        }
    }

    public override void Tick()
    {
#if UNITY_EDITOR
        if (InputHandler.GetKeyDown(KeyCode.Space))
        {
            ProductionStart();
        }
#endif
    }

    public override void Exit()
    {
        EventManager.Instance.OnScreenClick -= ProductionStart;
        if (_gameMode is BattleMode battleMode)
        {
            battleMode.DeathOrderQueue.Clear();
        }
    }
}
