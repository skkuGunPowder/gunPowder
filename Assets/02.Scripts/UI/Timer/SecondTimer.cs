using System;
using UnityEngine;

public class SecondTimer
{
    private float _currentTime;
    private int _lastSecond;
    private bool _isRunning;

    public Action<int> OnSecondChanged; // 초 변경 이벤트
    public Action OnTimerEnded;         // 타이머 종료 이벤트

    public SecondTimer(float startTime)
    {
        _currentTime = startTime;
        _lastSecond = Mathf.CeilToInt(_currentTime);
        _isRunning = true;
    }

    public void Tick(float deltaTime)
    {
        if (!_isRunning) return;

        _currentTime -= deltaTime;

        int currentSecond = Mathf.CeilToInt(_currentTime);

        // 🔥 1초 변화 감지
        if (currentSecond != _lastSecond)
        {
            _lastSecond = currentSecond;
            OnSecondChanged?.Invoke(Mathf.Max(0, currentSecond));
        }

        // 🔥 종료 처리
        if (_currentTime <= 0f)
        {
            _isRunning = false;
            OnTimerEnded?.Invoke();
        }
    }

    public int GetCurrentTime()
    {
        return _lastSecond;
    }

    public void Stop()
    {
        _isRunning = false;
    }
}