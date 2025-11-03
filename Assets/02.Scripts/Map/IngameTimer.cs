using System;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
public class IngameTimer : MonoBehaviour
{
    public UI_IngameTimer UI_Timer;
    private int _initTime;
    private float _timer;
    private int _previousTime;
    private bool _isGameOver;
    
    [Header("드랍 떨어지는 시간")] 
    public float AirDropTime = 30f;
    private float _airDropTimer;
    [SerializeField] private GameObject _airDropJetPrefab;
    private void Start()
    {
        _initTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.PlayTime.ToString()].ToString()) * 60;
        _timer = _initTime;
        _previousTime = _initTime;
        UI_Timer.RefreshTimer(_initTime);
        _isGameOver = false;
        GameManager.Instance.OnTimeCheck += TimeCheck;
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentGameState == EGameState.Playing || GameManager.Instance.CurrentGameState == EGameState.Result)
        {
            GameTimer();
        }
    }

    private void GameTimer()
    {
        _timer -= Time.deltaTime;
        _airDropTimer += Time.deltaTime;
    
        if (_timer <= 0)
        {
            if (_isGameOver == true)
            {
                return;
            }
            GameOver();
            UI_Timer.RefreshTimer(0);
        }

        int currentTime = Mathf.FloorToInt(_timer);
        
        // 정수 값이 변경되었을 때만 UI 갱신
        if (_previousTime != currentTime)
        {
            _previousTime = currentTime;
            UI_Timer.RefreshTimer(currentTime);
        }
    
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
    
        if (_airDropTimer > AirDropTime)
        {
            _airDropTimer = 0f;
        
            if (UnityEngine.Random.Range(0f, 1.0f) <= 0.1f)
            {
                PhotonNetwork.Instantiate(_airDropJetPrefab.name, transform.position, Quaternion.identity);
            }
        }

    }

    public void GameOver()
    {
        if (_isGameOver == true)
        {
            return;
        }
        
        _isGameOver = true;
        
        // 모든 클라이언트에서 모드에 타이머 종료 알림
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerExpired();
        }
    }

    private void TimeCheck(PhotonPlayer targetPlayer)
    {
        int playtime = (int)Mathf.Abs(_timer - _initTime);
        Hashtable hash = new Hashtable() 
        {
            {EProperties.SurvivorTime.ToString(), playtime} 
        };
            
        targetPlayer.SetCustomProperties(hash);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnTimeCheck -= TimeCheck;
    }
}
