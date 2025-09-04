using System;
using UnityEngine;
using TMPro;

public class UI_IngameTimer : MonoBehaviour
{
    public TextMeshProUGUI TimerTextMeshProUGUI;
    
    public void RefreshTimer(int timer)
    {
        TimerTextMeshProUGUI.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss");
    }
}
