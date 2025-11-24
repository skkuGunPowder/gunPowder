using System;
using UnityEngine;

public class FirstCheck : MonoBehaviour
{
    public bool _isFirst;
    public string TutorialMessage;
    
    private void Start()
    {
        _isFirst = PhotonServerManager.Instance.IsFirst;

        if (_isFirst == false)
        {
            return;
        }
        UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
        popup.Init(TutorialMessage,false, PhotonServerManager.Instance.TutorialMode);
        
        PhotonServerManager.Instance.SetFirst(false);
    }
    
    
}
