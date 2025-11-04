using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PopupSlot : MonoBehaviour
{
    public EPopupType Popup;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    public void Open()
    {
        PopupManager.Instance.Open(Popup, ButtonInteractable);
        _button.interactable = false;
    }
    
    private void ButtonInteractable()
    {
        _button.interactable = true;
    }

    public void Close()
    {
        PopupManager.Instance.Close(Popup);
    }
}
