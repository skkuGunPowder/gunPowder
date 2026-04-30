using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_RepairPopup : UI_Popup
{
    [SerializeField] private List<UI_RepairSlot> _slotList = new List<UI_RepairSlot>();
    private List<UI_RepairSlot> _slots = new List<UI_RepairSlot>();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        Dictionary<string, Cartridge> permanentDic = CartridgeInventoryManager.Instance.GetPermanentCartridges();
        List<Cartridge> cartridgeList = new List<Cartridge>(permanentDic.Values);
        
        for (int i = 0; i < _slotList.Count; i++)
        {
            if (i < cartridgeList.Count)
            {
                _slotList[i].gameObject.SetActive(true);
                _slotList[i].Refresh(cartridgeList[i]);
            }
            else
            {
                _slotList[i].gameObject.SetActive(false);
            }
        }
    }

    // // 수리 완료 후 다른 슬롯의 수리 비용 텍스트 갱신 필요 없음 (슬롯마다 자체 업데이트)
    // private void OnSlotRepaired()
    // {
    //     //
    // }
}
