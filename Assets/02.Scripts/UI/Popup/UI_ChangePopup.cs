using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_ChangePopup : UI_Popup
{
    [SerializeField] private List<UI_ChangeSlot> _slotList = new List<UI_ChangeSlot>();

    private CartridgeData _pendingData;
    private int _pendingPrice;
    private Action _onExchanged;

    public void Setup(CartridgeData data, int price, Action onExchanged)
    {
        _pendingData = data;
        _pendingPrice = price;
        _onExchanged = onExchanged;
        Refresh();
    }

    private void Refresh()
    {
        // 소모형 + 영구형 전부 합쳐서 슬롯에 표시
        List<Cartridge> allCartridges = new List<Cartridge>();
        allCartridges.AddRange(CartridgeInventoryManager.Instance.GetConsumableCartridges().Values);
        allCartridges.AddRange(CartridgeInventoryManager.Instance.GetPermanentCartridges().Values);

        for (int i = 0; i < _slotList.Count; i++)
        {
            if (i < allCartridges.Count)
            {
                _slotList[i].gameObject.SetActive(true);
                int index = i;
                _slotList[i].Refresh(allCartridges[i], () => TryExchange(allCartridges[index].Data.ID));
            }
            else
            {
                _slotList[i].gameObject.SetActive(false);
            }
        }
    }

    private void TryExchange(string removeId)
    {
        if (!CartridgeInventoryManager.Instance.SwapCartridge(removeId, _pendingData.ID))
        {
            return;
        }

        RoomStatManager.Instance.ChangeGunpowder(-_pendingPrice);
        _onExchanged?.Invoke();
        Close();
    }


    private void OnDisable()
    {
        _pendingData = null;
        _pendingPrice = 0;
        _onExchanged = null;
    }
}
