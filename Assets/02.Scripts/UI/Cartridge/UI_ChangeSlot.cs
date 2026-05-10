using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeSlot : UI_CartridgeSlotBase
{
    private const string CHANGE_NAME = "교체";

    [SerializeField] private Button _changeButton;

    private Action _onSelected;

    public void Refresh(Cartridge cartridge, Action onSelected)
    {
        _onSelected = onSelected;
        RefreshBaseUI(cartridge, CHANGE_NAME);
        _changeButton.interactable = true;
    }

    public void OnClickExchange()
    {
        _onSelected?.Invoke();
    }
}
