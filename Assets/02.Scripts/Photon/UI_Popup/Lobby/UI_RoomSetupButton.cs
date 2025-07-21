using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoomSetupButton : MonoBehaviour
{
    public TextMeshProUGUI Value;
    public int MaxValue;           // 최대치는 직접 입력해서 조절
    public int MinValue;           // 최소치는 직접 입력해서 조절하기
    public int InitValue;          // 처음 시작 값 조절
    private int _currentValue;      // 현재 값

    public void Init()
    {
        _currentValue = InitValue;
        Refresh();
    }

    private void OnDisable()
    {
        _currentValue = InitValue;
    }

    public void ValueUpDown(int value)
    {
        _currentValue = Mathf.Clamp(_currentValue + value, MinValue, MaxValue);
        
        Refresh();
    }

    public void Refresh()
    {
        Value.text = _currentValue.ToString();
    }

    public int CurrentValue()
    {
        return _currentValue;
    }
}
