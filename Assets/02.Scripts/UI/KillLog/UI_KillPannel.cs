using System;
using TMPro;
using UnityEngine;

public class UI_KillPannel : MonoBehaviour
{
    public TextMeshProUGUI KillPannelText;
    [SerializeField] private float _time = 1f;
    private float _timer;
    
    public void Refresh(string playerNickname)
    {
        KillPannelText.text = playerNickname;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        
        if (_timer >= _time)
        {
            _timer = 0;
            this.gameObject.SetActive(false);
        }
    }
}
