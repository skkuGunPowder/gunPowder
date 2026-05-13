using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameStateCartridge : GameModeStateBase
{
    [SerializeField] private float _longtime = 8;
    [SerializeField] private float _shortTime = 5;
    
    private SecondTimer _timer;
    private int _currentTurnIndex;
    private bool _isLastTurn;
    
    public override void Enter()
    {
        PopupManager.Instance.Open(EPopupType.UI_CartridgeShopPopup);
        EventManager.Instance.CartridgeStateEnter();

        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        _currentTurnIndex = 0;
        StartTurn();
    }

    // 방장만 실행
    private void StartTurn()
    {
        if (_gameMode is BattleMode battleMode)
        {
            // GP 음수 플레이어 전부 스킵
            while (battleMode.DeathOrderQueue.Count > 0)
            {
                PhotonPlayer candidate = battleMode.DeathOrderQueue.Peek();
                bool negativeGP = candidate.CustomProperties.ContainsKey(EProperties.GP.ToString()) &&
                                  (int)candidate.CustomProperties[EProperties.GP.ToString()] < 0;
                if (negativeGP == false)
                {
                    break;
                }
                battleMode.DeathOrderQueue.Dequeue();
            }

            if (battleMode.DeathOrderQueue.Count == 0)
            {
                FinishCartridge();
                return;
            }

            PhotonPlayer player = battleMode.DeathOrderQueue.Dequeue();

            bool isFirstTurn = _currentTurnIndex == 0;
            _isLastTurn = battleMode.DeathOrderQueue.Count == 0;
            _currentTurnIndex++;
            // 마지막 턴이 아닌 경우 행동(OnScreenClick) 시 즉시 다음 턴으로 넘김
            if (_isLastTurn == false)
            {
                EventManager.Instance.OnScreenClick += OnTurnAction;
            }

            float duration = (isFirstTurn || _isLastTurn) ? _longtime : _shortTime;
            // 모든 클라이언트에 턴 시작 알림 (타이머 생성 포함)
            _photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, player.ActorNumber, (int)duration);
        }
    }

    [PunRPC]
    private void RPC_StartTurn(int actorNumber, int time)
    {
        PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        EventManager.Instance.TimeSet(time);
        if (player != null)
        {
            EventManager.Instance.CartridgeStart(player);
        }

        // 모든 클라이언트가 로컬 타이머 실행 — 방장 이탈 시 새 방장이 이어받을 수 있도록
        _timer?.Destroy();
        _timer = new SecondTimer(time, OnLocalTimerEnd, (sec) => EventManager.Instance.TimerUpdate(sec));
    }

    // 턴 소모 행동 발생 시 호출 (방장만 구독)
    private void OnTurnAction()
    {
        EventManager.Instance.OnScreenClick -= OnTurnAction;
        _timer?.Destroy();
        _timer = null;
        StartTurn();
    }

    // 로컬 타이머 만료 시 호출 (모든 클라이언트) — 방장만 턴 전환
    private void OnLocalTimerEnd()
    {
        _timer = null;
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        EventManager.Instance.OnScreenClick -= OnTurnAction;
        StartTurn();
    }

    // 카트리지 끝 
    private void FinishCartridge()
    {
        _gameMode.RequestStateChange(EModeState.Spawn);
    }

    public override void Tick()
    {
        // 모든 클라이언트가 로컬 타이머 진행
        _timer?.Tick(Time.deltaTime);

#if UNITY_EDITOR
        if (PhotonNetwork.IsMasterClient && InputHandler.GetKeyDown(KeyCode.Space))
        {
            OnTurnAction();
        }
#endif
    }
    
    
    public override void Exit()
    {
        EventManager.Instance.OnScreenClick -= OnTurnAction;
        _timer?.Destroy();
        _timer = null;
        
        EventManager.Instance.CartridgeEnd();
        
        if (_gameMode is BattleMode battleMode)
        {
            battleMode.DeathOrderQueue.Clear();
        }
    }
}
