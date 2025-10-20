using System;
using UnityEngine;

public abstract class UI_Popup : MonoBehaviour
{
    private Action _closeCallback;

    public void Open(Action closeCallback = null)
    {
        _closeCallback = closeCallback;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        _closeCallback?.Invoke();
        gameObject.SetActive(false);
    }
    
}
