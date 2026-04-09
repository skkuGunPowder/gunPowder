using System;
using UnityEngine;

public class SecondTimer
{
    private float _initTime;
    private float _currentTime;
    private int _lastSecond;
    private bool _isRunning;

    public Action<int> OnSecondChanged; // 초 변경 이벤트
    public Action OnTimerEnded;         // 타이머 종료 이벤트

    public SecondTimer(float startTime, Action endAction, Action<int> changeAction)
    {
        _initTime = startTime;
        _currentTime = startTime;
        _lastSecond = Mathf.CeilToInt(_currentTime);
        _isRunning = true;
        OnSecondChanged = changeAction;
        OnTimerEnded = endAction;
    }

    public void Tick(float deltaTime)
    {
        if (!_isRunning) return;
        _currentTime -= deltaTime;
        
        Debug.Log($"Second TimerTick{_currentTime}");

        int currentSecond = Mathf.CeilToInt(_currentTime);

        // 🔥 1초 변화 감지
        if (currentSecond != _lastSecond)
        {
            _lastSecond = currentSecond;
            OnSecondChanged?.Invoke(Mathf.Max(0, _lastSecond));
        }

        // 🔥 종료 처리
        if (_currentTime <= 0f)
        {
            _isRunning = false;
            OnTimerEnded?.Invoke();
            Destroy();
        }
    }

    public int GetSurviveTime()
    {
        int playtime = (int)Mathf.Abs(_lastSecond - _initTime);

        return playtime;
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Destroy()
    {
        Stop();
        OnSecondChanged = null;
        OnTimerEnded = null;
    }
}