using System;
using UnityEngine;
using TMPro;
public class UI_IngameTimer : MonoBehaviour
{
    public TextMeshProUGUI TimerTextMeshProUGUI;
    private float _timer;
    private void Update()
    {
        _timer = GameManager.Instance.Timer;
        
        if (_timer <= 0)
        {
            _timer = 0;
            TimerTextMeshProUGUI.text = TimeSpan.FromSeconds(_timer).ToString(@"mm\:ss");
            
        }
        TimerTextMeshProUGUI.text = TimeSpan.FromSeconds(_timer).ToString(@"mm\:ss");

    }
}
