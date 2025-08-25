using Photon.Pun;
using UnityEngine;

public class PopupSlot : MonoBehaviour
{
    public EPopupType Popup;

    public void Open()
    {
        PopupManager.Instance.Open(Popup);
    }

    public void Close()
    {
        PopupManager.Instance.Close(Popup);
    }
}
