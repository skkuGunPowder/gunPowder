using System;
using UnityEngine;

public class UI_Toast : MonoBehaviour
{
    public virtual void Open(float duration, string message, Action callback = null)
    {
        gameObject.SetActive(true);
    }
}
