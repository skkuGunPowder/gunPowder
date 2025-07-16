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
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (RoomManager.Instance.IsPlayerReady() == false)
            {
                return;
            }   
        }
        
        _isReady = !_isReady;
        
        Hashtable ready = new Hashtable { {"isReady" , _isReady} };
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);

        ReadyTextUGUI.text =  _isReady ? "Ready" : "Not Ready";

        Debug.Log(ready["isReady"]);
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        RoomManager.Instance.GameStart();
    }
}
