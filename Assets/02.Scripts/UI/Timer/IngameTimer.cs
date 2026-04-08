using System;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
public class IngameTimer : TimerBase
{
    private float _timer;
    private int _previousTime;
    private bool _isGameOver;

    private const float HURRY_UP_TIME = 30f;
    private bool _isHurryUp = false;
    
    private void Start()
    {
        int time = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.PlayTime.ToString()].ToString()) * 60;
        SetTime(time, 0);
        _timer = time;
        UI_Timer.TextRefresh(ConvertTime(time));
        _isGameOver = false;
    }

    // 타이머용 text에 표시될 내용
    protected override string ConvertTime(int time)
    {
        string timeText = TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
        
        return timeText;
    }

    protected override void TimeChange(int time)
    {
        if (!_isHurryUp && time <= HURRY_UP_TIME && time > 0)
        {
            _isHurryUp = true;
            EventManager.Instance.HurryUp();
        }

        if (time <= 0)
        {
            if (_isGameOver == true)
            {
                return;
            }
            
            EndTimeAction();
            UI_Timer.TextRefresh(ConvertTime(0));
        }


        UI_Timer.TextRefresh(ConvertTime(time));
    }

    protected override void EndTimeAction()
    {
        _isGameOver = true;
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        EventManager.Instance.TimerEnded();
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

    protected override void UnSubScribe()
    {
        base.UnSubScribe();
        EventManager.Instance.OnTimeCheck -= TimeCheck;
    }
    
    protected override void SubScribe()
    {
        base.SubScribe();
        EventManager.Instance.OnTimeCheck += TimeCheck;
    }
}
