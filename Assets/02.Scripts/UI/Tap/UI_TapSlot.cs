using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_TapSlot : MonoBehaviour
{
    public ETapOption TapType;
    public GameObject TapObject;
    public Button TapButton;
    public bool IsMaster;

    // 받은 인자랑 내 type이 같으면 true 아니면 false
    public void SetActive(ETapOption type)
    {
        if(type == TapType)
        {
            TapObject.SetActive(true);
            TapButton.interactable = false;
        }
        else
        {
            TapObject.SetActive(false);
            TapButton.interactable = true;       
        }
    }
}
