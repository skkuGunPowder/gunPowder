using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("팝업 UI")]
    public List<UI_Popup> PopupList = new List<UI_Popup>();
    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    public static PopupManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Instance.PopupList = PopupList;
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_popupStack.Count > 0)
            {
                while (true)
                {
                    UI_Popup popup = _popupStack.Pop();
                    bool opened = popup.isActiveAndEnabled;
                    popup.Close();

                    if (opened || _popupStack.Peek() == null)
                    {
                        break;
                    }
                }
            }
            else
            {
                Open(EPopupType.UI_SystemPopup);
            }
        }
    }

    public UI_Popup Open(EPopupType popupType, Action closeCallback = null)
    {
        return PopupOpen(popupType.ToString(), closeCallback);
    }

    private UI_Popup PopupOpen(string popupName, Action closeCallback)
    {
        foreach (UI_Popup popup in PopupList)
        {
            if (popup.name == popupName)
            {
                popup.Open(closeCallback);
                _popupStack.Push(popup);
                return popup;
            }
        }
        return null;
    }

    public void Close(EPopupType popupType)
    {
        PopupClose(popupType.ToString());
    }
    private void PopupClose(string popupName)
    {
        foreach (UI_Popup popup in PopupList)
        {
            if (popup.name == popupName)
            {
                popup.Close();
                break;
            }
        }

    }
}
