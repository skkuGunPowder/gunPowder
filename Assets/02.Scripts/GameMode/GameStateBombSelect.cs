using Photon.Pun;
using UnityEngine;

public class GameStateBombSelect : GameModeStateBase
{
    [SerializeField] private float _bombSelectDuration = 10f;

    private float _timer;
    private bool _phaseStarted;
    private bool _stateChangeRequested;

    public static bool IsActive { get; private set; } // 현재 BombSelect 상태인지 여부

    public override void Enter()
    {
        Debug.LogWarning("Enter State : GameStateBombSelect");
        IsActive = true;
        _phaseStarted = false;
        _stateChangeRequested = false;

        OnPhaseStart();
        
        // 아이템 보관함 열기 (닫을 때 인풋 해제)
        PopupManager.Instance.Open(EPopupType.UI_ItemStorage, () => InputHandler.BlockInput = false);
    }

    private void OnPhaseStart()
    {
        _timer = _bombSelectDuration;
        _phaseStarted = true;
    }

    public override void Tick()
    {
        if (!_phaseStarted || _stateChangeRequested) return;

        _timer -= Time.unscaledDeltaTime;

        if (_timer <= 0f && PhotonNetwork.IsMasterClient)
        {
            _stateChangeRequested = true;
            _gameMode.RequestStateChange(EModeState.Playing);
        }
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
