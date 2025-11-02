using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToLobbyButton : MonoBehaviour
{
    [SerializeField] private float _clickDelay = 0.5f; 
    private Button _button;
    private void Start()
    {
        _button = GetComponent<Button>();
    }
    
    public void OnClickToLobby()
    {
        StartCoroutine(ButtonClick_Coroutine());
        PhotonNetwork.LeaveRoom();
    }

    private IEnumerator ButtonClick_Coroutine()
    {
        _button.interactable = false;
        
        yield return _clickDelay;
        
        _button.interactable = true;
    }
    
    private void OnDestroy()
    {
        StopCoroutine(ButtonClick_Coroutine());    
    }
}
