
using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ReadyButton : MonoBehaviour
{
    private bool _isReady = false;
    [SerializeField] private Button _button;
    public TextMeshProUGUI ReadyTextUGUI;

    public string Ready = "준비 완료";
    public string NotReady = "준비";

    public string Master = "시작";

    private void OnEnable()
    {
        Reset();
    }
    
    private void Reset()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ReadyTextUGUI.text = Master;
            
        }
        else
        {
            ReadyTextUGUI.text = NotReady;
        }
    }

    private void Update()
    {
        if (InputHandler.GetKeyDown(KeyCode.F5))
        {
            OnClickReady();   
        }
    }

    // 레디 버튼을 눌렀을 때, 커스텀 프로퍼티를 바꾼다.
    public void OnClickReady()
    {
        //만약 내가 방장이라면 레디 자체를 안눌리게 한다.
        if (PhotonNetwork.IsMasterClient)
        {
            if (RoomManager.Instance.ReadyCheck.IsPlayerReady() == false)
            {
                return;
            }
            // 버튼 잠금 : 더블 클릭 방지
            _button.interactable = false;
            RoomManager.Instance.GameStart();
        }
        
        // 레디 했다가 안했다가 할 수 있다.
        _isReady = !_isReady;
        
        Hashtable ready = new Hashtable
        {
            {EProperties.IsReady.ToString() , _isReady}
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);

        ReadyTextUGUI.text = _isReady ? Ready : NotReady;
    }
}