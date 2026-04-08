using System;
using UnityEngine;

public class TimerBase : MonoBehaviour
{
    [SerializeField] protected UI_TextSlot UI_Timer;
    protected int _initTime; 
    protected int _endTime;
    private void OnEnable()
    {
        // 타이머용 이벤트
        SubScribe();
    }

    private void OnDisable()
    {
        UnSubScribe();
    }

    // 시작 시간 정하기
    protected void SetTime(int initTime, int endTime)
    {
        _initTime = initTime;
        _endTime = endTime;
    }

    protected virtual void TimeChange(int time)
    {
        
    }

    // 타이머용 text에 표시될 내용
    protected virtual string ConvertTime(int time)
    {
        return time.ToString();
    }

    protected virtual void EndTimeAction()
    {
        // 시간이 끝났을 때 해야할 행동
    }

    protected virtual void SubScribe()
    {
        EventManager.Instance.OnTimerUpdate += TimeChange;
    }
    
    protected virtual void UnSubScribe()
    {
        EventManager.Instance.OnTimerUpdate -= TimeChange;
    }
}
