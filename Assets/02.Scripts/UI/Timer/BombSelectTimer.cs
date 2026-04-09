public class BombSelectTimer : TimerBase
{
    protected override void SubScribe()
    {
        EventManager.Instance.OnBombSelectTimerTick += TimeChange;
    }

    protected override void UnSubScribe()
    {
        EventManager.Instance.OnBombSelectTimerTick -= TimeChange;
    }

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
        string remain = $"폭탄 선택까지 {time}초 남았습니다.";
        return remain;
    }
}
