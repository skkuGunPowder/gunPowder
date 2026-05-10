using UnityEngine;
using UnityEngine.UI;

public class UI_RepairSlot : UI_CartridgeSlotBase
{
    private const string REPAIR_NAME = "수리";

    [SerializeField] private Button _repairButton;

    public void Refresh(Cartridge cartridge)
    {
        RefreshBaseUI(cartridge, REPAIR_NAME);
        UpdateButton();
    }

    private void UpdateButton()
    {
        int currentDurability = _cartridge.GetCurrentDurability();
        int maxDurability = _cartridge.GetMaxDurability();
        int repairCost = _cartridge.Data.GetRepairCost(_cartridge.GetRepairCount());

        // 이미 수리했거나, 내구도가 최대이거나, 빚 상태면 버튼 비활성화
        bool canRepair = !_cartridge.IsRepairedThisTurn()
                         && currentDurability < maxDurability
                         && RoomStatManager.Instance.CanChangeGP(-repairCost);

        _repairButton.interactable = canRepair;
    }

    public void OnClickRepair()
    {
        if (CartridgeInventoryManager.Instance.TryRepairWithCost(_cartridge.Data.ID))
        {
            UpdateButton();
        }
    }
}
