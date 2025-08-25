using TMPro;
using System;
using UnityEngine.UI;

public class UI_MessagePopup : UI_Popup
{
    public Button OKButton;
    public Button CancleButton;
    public TextMeshProUGUI MessageText;

    private Action _callback;

    public void Init(string message, bool isCancleButton, Action callback = null)
    {
        SetText(message);
        if (callback != null)
        {
            SetCallback(callback);
        }

        if (isCancleButton)
        {
            OKButton.gameObject.SetActive(true);
            CancleButton.gameObject.SetActive(true);
        }
        else
        {
            OKButton.gameObject.SetActive(true);
            CancleButton.gameObject.SetActive(false);
        }
    }

    public void SetText(string message)
    {
        MessageText.text = message;
    }

    public void SetCallback(Action callback)
    {
        _callback = callback;
    }

    public void OnClickOK()
    {
        _callback?.Invoke();
        Close();
    }

    public void OnClickCancle()
    {
        Close();
    }
}
