using System.Collections.Generic;
using UnityEngine;
using System;

public enum EToastType
{
    UI_Toast,
    UI_GameTimeScroll
}

public class ToastMessageManager : DontDestroySingleton<ToastMessageManager>
{
    [SerializeField] private List<UI_Toast> _toastList = new List<UI_Toast>();
    [SerializeField] private Canvas _currentSceneCanvas;

    protected override void Awake()
    {
        base.Awake();
        _currentSceneCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
    }

    public UI_Toast Open(EToastType toastType, string message, Action closeCallback = null)
    {
        if (_currentSceneCanvas == null)
        {
            _currentSceneCanvas = FindAnyObjectByType<Canvas>();
        }
        
        return ToastOpen(toastType.ToString(), message, closeCallback);
    }

    private UI_Toast ToastOpen(string toastName, string message, Action closeCallback)
    {
        foreach (UI_Toast toast in _toastList)
        {
            if (toast.name == toastName)
            {
                UI_Toast instanceToast = Instantiate(toast.gameObject, _currentSceneCanvas.transform).GetComponent<UI_Toast>();
                instanceToast.Open(2.0f, message, closeCallback);
                return toast;
            }
        }
        Debug.LogError($"[ToastMessageManager] 토스트 메시지를 찾을 수 없습니다: {toastName}");
        return null;
    }
}
