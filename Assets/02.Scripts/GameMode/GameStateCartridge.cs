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
            // 모든 클라이언트에 턴 시작 알림
            _photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, player.ActorNumber, (int)duration);
            
            //타이머 생성은 방장만
            _timer?.Destroy();
            _timer = new SecondTimer(duration, OnTimerEnd, RequestChangeTime);
        }
    }

    [PunRPC]
    private void RPC_StartTurn(int actorNumber, int time)
    {
        PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        
        // 타이머 세팅
        EventManager.Instance.TimeSet(time);
        if (player != null)
        {
            EventManager.Instance.CartridgeStart(player);
        }
    }

    // 턴 소모 행동 발생 시 호출 (방장만 구독)
    private void OnTurnAction()
    {
        EventManager.Instance.OnScreenClick -= OnTurnAction;
        _timer?.Destroy();
        _timer = null;
        StartTurn();
    }

    // 타이머 만료 시 호출 (방장만)
    private void OnTimerEnd()
    {
        EventManager.Instance.OnScreenClick -= OnTurnAction;
        _timer = null;
        StartTurn();
    }

    // 카트리지 끝 
    private void FinishCartridge()
    {
        _gameMode.RequestStateChange(EModeState.Spawn);
    }

    public override void Tick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _timer?.Tick(Time.deltaTime);
        }

#if UNITY_EDITOR
        if (PhotonNetwork.IsMasterClient && InputHandler.GetKeyDown(KeyCode.Space))
        {
            OnTurnAction();
        }
#endif
    }


    // 시간 조절
    private void RequestChangeTime(int time)
    {
        _photonView.RPC(nameof(RPC_ChangeTime), RpcTarget.All, time);
    }

    [PunRPC]
    public void RPC_ChangeTime(int time)
    {  
        EventManager.Instance.TimerUpdate(time);
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
