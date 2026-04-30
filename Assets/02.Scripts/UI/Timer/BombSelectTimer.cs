using UnityEngine;

public class BombSelectTimer : TimerBase
{
    [SerializeField] private string _remainTime;
    
    protected override void TimeChange(int time)
    {
        if (time <= 0)
        {
            EndTimeAction();
            UI_Timer.TextRefresh(ConvertTime(0));
            return;
        }

        UI_Timer.TextRefresh(ConvertTime(time));
    }
    
    protected override string ConvertTime(int time)
    {
        return string.Format(_remainTime, time);
    }
}
