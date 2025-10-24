using UnityEngine;

public class UI_MenuPopup : UI_Popup
{
    public void OnClickExit()
    {
        ClientManager.Quit();
    }
}
