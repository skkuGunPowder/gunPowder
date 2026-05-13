using Photon.Pun;
using UnityEngine;

public class GameStateBombSelect : GameModeStateBase
{
    [SerializeField] private int _bombSelectDuration = 10;
    
    private bool _phaseStarted;
    private bool _stateChangeRequested;
    
    private SecondTimer _secondTimer;

    public static bool IsActive { get; private set; } // 현재 BombSelect 상태인지 여부

    public override void Enter()
    {
        IsActive = true;
        _phaseStarted = false;
        _stateChangeRequested = false;
        InputHandler.SystemBlock = false; // 시스템 핸들어 해제
        OnPhaseStart();
        
        // 아이템 보관함 열기 (닫을 때 인풋 해제)
        PopupManager.Instance.Open(EPopupType.UI_TempStorage);
        
        // 시간 설정
        int second = _bombSelectDuration;
        
        _secondTimer = new SecondTimer(second,OnTimeOver,
            (sec) => EventManager.Instance.TimerUpdate(sec)
        );
    }

    private void OnPhaseStart()
    {
        InputHandler.BlockInput = true;
        _phaseStarted = true;
    }

    public override void Tick()
    {
        _secondTimer?.Tick(Time.unscaledDeltaTime);
    }

    private void OnTimeOver()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        _stateChangeRequested = true;
        _gameMode.RequestStateChange(EModeState.Playing);
    }

    public override void Exit()
    {
        IsActive = false;
        _phaseStarted = false;
        _stateChangeRequested = false;
        _gameMode.SetFirstSpawnComplete(); // 첫 스폰 완료 표시 (이후 라운드는 BombSelect 건너뜀)
        // 아이템 보관함을 열어둔 플레이어는 직접 닫을 때 인풋이 해제됨 (Close 강제 호출 안함)
    }
}
