using UnityEngine;

public class IngameChatManager : PhotonSingleton<IngameChatManager>
{
    public UI_IngameChat UI_IngameChatPopup;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PopupManager.Instance.Open(EPopupType.UI_IngameChatPopup);
        }
    }
}
