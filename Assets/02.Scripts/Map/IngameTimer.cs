using System;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
public class IngameTimer : MonoBehaviour
{
    public UI_TextSlot UI_Timer;
    private int _initTime;
    private float _timer;
    private int _previousTime;
    private bool _isGameOver;

    private const float HURRY_UP_TIME = 30f;
    private bool _isHurryUp = false;
    
    private void Start()
    {
        _initTime = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.PlayTime.ToString()].ToString()) * 60;
        _timer = _initTime;
        _previousTime = _initTime;
        UI_Timer.TextRefresh(ConvertTime(_initTime));
        _isGameOver = false;
        EventManager.Instance.OnTimeCheck += TimeCheck;
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentGameState == EGameState.Playing || GameManager.Instance.CurrentGameState == EGameState.Result)
        {
            GameTimer();
        }
    }

    private string ConvertTime(int time)
    {
        string timeText = TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        
        return timeText;
    }

    private void GameTimer()
    {
        _timer -= Time.deltaTime;

        if (!_isHurryUp && _timer <= HURRY_UP_TIME && _timer > 0)
        {
            _isHurryUp = true;
            EventManager.Instance.HurryUp();
        }

        if (_timer <= 0)
        {
            if (_isGameOver == true)
            {
                return;
            }
            GameOver();
            UI_Timer.TextRefresh(ConvertTime(0));
        }

        int currentTime = Mathf.FloorToInt(_timer);

        if (_previousTime != currentTime)
        {
            _previousTime = currentTime;
            UI_Timer.TextRefresh(ConvertTime(currentTime));
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        GameManager.Instance.RequestGameOver();
    }

    private void TimeCheck(PhotonPlayer targetPlayer)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
     
        int playtime = (int)Mathf.Abs(_timer - _initTime);
        Hashtable hash = new Hashtable() 
        {
            {EProperties.SurvivorTime.ToString(), playtime} 
        };
            
        targetPlayer.SetCustomProperties(hash);
    }

    private void OnDisable()
    {
        EventManager.Instance.OnTimeCheck -= TimeCheck;
    }
}
