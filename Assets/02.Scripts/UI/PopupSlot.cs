using UnityEngine;

public class PopupSlot : MonoBehaviour
{
    public EPopupType Popup;

    public void Open()
    {
        PopupManager.Instance.Open(Popup);
    }
}
