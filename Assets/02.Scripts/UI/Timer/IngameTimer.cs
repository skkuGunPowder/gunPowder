using System;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
public class IngameTimer : TimerBase
{
    private int _previousTime;
    private bool _isGameOver;

    private const float HURRY_UP_TIME = 30f;
    private bool _isHurryUp = false;
    
    private void Start()
    {
        int time = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.PlayTime.ToString()].ToString()) * 60;
        SetTime(time, 0);
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
            return;
        }

        UI_Timer.TextRefresh(ConvertTime(time));
    }

    protected override void EndTimeAction()
    {
        _isGameOver = true;
        UnSubScribe();
    }
}
