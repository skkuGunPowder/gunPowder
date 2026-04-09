using UnityEngine;
using UnityEngine.UI;

public class UI_TempStorage : UI_ItemStorage
{
    public UI_TempEquipmentSlot MainEquipmentSlot;
    public UI_TempEquipmentSlot SubEquipmentSlot;

    [SerializeField] private Button _button;
    private bool _initialized = false;
    
    protected override void Refresh(EItemType currentCategory)
    {
        base.Refresh(currentCategory);
        
        if (!_initialized)
        {
            _initialized = true;
            UnEquipBombSlots();
            return;
        }
        
        if (currentCategory == EItemType.Bomb)
        { 
            SubEquipmentSlot.Refresh(_itemStorage.GetEquippedSubBomb());   
        }

        UnLockConfirmButton();
    }

    private void UnEquipBombSlots()
    {
        InventoryItem bomb = _itemStorage.GetEquppedItem(EItemType.Bomb);
        if (bomb != null)
        {
            _itemStorage.UnEquipItem(bomb);
        }
        
        _itemStorage.UnEquipSubBomb();
    }
    
    protected override void EquipmentSlotRefresh(EItemType currentCategory)
    {
        InventoryItem EquippedItem = _itemStorage.GetEquppedItem(currentCategory);
        MainEquipmentSlot.Refresh(EquippedItem);
    }

    private void UnLockConfirmButton()
    {
        if (SubEquipmentSlot.Selected && MainEquipmentSlot.Selected)
        {
            _button.interactable = true;
        }
        else
        {
            _button.interactable = false;
        }
    }
    public void OnConfirm()
    {
        PopupManager.Instance.Close(EPopupType.UI_TempStorage);
    }
}
