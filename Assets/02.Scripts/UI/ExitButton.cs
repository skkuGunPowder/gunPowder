using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void OnClickExit()
    {
        UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
        //messagePopup.Init("종료하시겠습니까?", true, ExitGame);
        messagePopup.Init("終了しますか？", true, ExitGame);
    }
    
    public void ExitGame()
    {
        ClientManager.Quit();
    }
}
