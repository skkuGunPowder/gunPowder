using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [Header("팝업 UI")]
    public List<UI_Popup> PopupList = new List<UI_Popup>();
    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

    private UI_IngameChatPopup _ingameChatPopup = null;

    protected override void Awake()
    {
        base.Awake();

        // 인게임 채팅 팝업 레퍼런스 캐싱
        foreach (UI_Popup popup in PopupList)
        {
            if (popup is UI_IngameChatPopup)
            {
                _ingameChatPopup = popup as UI_IngameChatPopup;
                break;
            }
        }
    }

    private void Update()
    {
        // Enter 키로 인게임 채팅창 열기
        if (InputHandler.GetSystemKeyDown(KeyCode.Return) || InputHandler.GetSystemKeyDown(KeyCode.KeypadEnter))
        {
            if (_ingameChatPopup != null && _ingameChatPopup.TryOpen())
            {
                Open(EPopupType.UI_IngameChatPopup);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_popupStack.Count > 0)
            {
                while (true)
                {
                    UI_Popup popup = _popupStack.Pop();
                    bool opened = popup.isActiveAndEnabled;
                    popup.Close();

                    if (opened) // 열려 있는 팝업이 있으면 멈추기
                    {
                        break;
                    }

                    if (_popupStack.Count == 0) // 열려있는 팝업이 없으면 바로 메뉴 팝업이 등장하도록
                    {
                        Open(EPopupType.UI_MenuPopup);
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

                if (popup.CanCloseESC)
                {
                    _popupStack.Push(popup);   
                }
                
                return popup;
            }
        }
        Debug.LogError($"[PopupManager] 팝업을 찾을 수 없습니다: {popupName}");
        return null;
    }

    // 팝업 체크할 때 가져오기
    public UI_Popup GetPopup(EPopupType popupType)
    {
        foreach (UI_Popup popup in PopupList)
        {
            if (popup.name != popupType.ToString())
            {
                continue;
            }
            
            return popup;
        }
        
        
        Debug.LogError($"[PopupManager] 팝업을 찾을 수 없습니다: {popupType.ToString()}");
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
