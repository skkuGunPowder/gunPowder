
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UI_ReadyButton : MonoBehaviour
{
    private bool _isReady = false;

    public TextMeshProUGUI ReadyTextUGUI;
    
    // 레디 버튼을 눌렀을 때, 커스텀 프로퍼티를 바꾼다.
    public void OnClickReady()
    {
        //만약 내가 방장이라면 레디 자체를 안눌리게 한다.
        if (PhotonNetwork.IsMasterClient)
        {
            if (RoomManager.Instance.IsPlayerReady() == false)
            {
                return;
            }   
        }
        
        // 레디 했다가 안했다가 할 수 있다.
        _isReady = !_isReady;
        
        Hashtable ready = new Hashtable { {$"{EProperties.IsReady}" , _isReady} };
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);

        ReadyTextUGUI.text =  _isReady ? "Ready" : "Not Ready";
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        RoomManager.Instance.GameStart();
    }
}