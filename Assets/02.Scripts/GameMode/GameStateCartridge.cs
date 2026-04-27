using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameStateCartridge : GameModeStateBase
{
    public override void Enter()
    {
        EventManager.Instance.OnScreenClick += ProductionStart;
        
        PopupManager.Instance.Open(EPopupType.UI_CartridgeShopPopup);
        EventManager.Instance.CartridgeStateEnter();
        ProductionStart();
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
                return;
            }

            // 모든 선택 완료
            UI_CartridgeShopPopup popup = (UI_CartridgeShopPopup)PopupManager.Instance.GetPopup(EPopupType.UI_CartridgeShopPopup);
            popup.Close();

            if (PhotonNetwork.IsMasterClient == false)
            {
                return;
            }
            
            // 방장 : 스테이트 변경
            _gameMode.RequestStateChange(EModeState.Spawn);
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
