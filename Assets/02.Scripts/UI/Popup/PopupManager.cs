using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [Header("팝업 UI")]
    public List<UI_Popup> PopupList = new List<UI_Popup>();
    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

    protected override void Awake()
    {
        base.Awake();
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

                    if (opened || _popupStack.Count == 0)
                    {
                        break;   
                    }
                }
            }
            else
            {
                Open(EPopupType.UI_MenuPopup);
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
        Debug.LogError($"[PopupManager] 팝업을 찾을 수 없습니다: {popupName}");
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
