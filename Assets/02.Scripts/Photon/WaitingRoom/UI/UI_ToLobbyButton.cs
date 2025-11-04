using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToLobbyButton : MonoBehaviour
{
    private float _clickDelay = 1f; 
    private float _timer = 0;
    private bool _isClick = false;
    private Button _button;
    private void Start()
    {
        _timer = 0;
        _button = GetComponent<Button>();
    }

    private void Update()
    {
        if (_isClick == false)
        {
            return;
        }
        
        _timer += Time.deltaTime;
        
        if (_timer >= _clickDelay)
        {
            _button.interactable = true;
            _isClick = false;
            _timer = 0;
        }
    }
    
    public void OnClickToLobby()
    {
        _isClick = true;
        _button.interactable = false;
        PhotonNetwork.LeaveRoom();
    }
}
