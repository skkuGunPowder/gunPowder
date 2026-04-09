using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemStorage : UI_Popup
{
    public TextMeshProUGUI ItemNameText;
    public UI_EquipmentSlot EquipmentSlot;
    public UI_Category UI_Category;

    [SerializeField] private List<UI_ItemSlot> _itemSlotList;
    [SerializeField] private bool _isFixed; // 카테고리 고정
    [SerializeField] private EMainCategory _fixedMainCategory;
    [SerializeField] private EItemType _fixedItemType;

    protected ItemStorage _itemStorage;
    private UI_ItemSlot _selectedSlot;

    
    private void Start()
    {
        _itemStorage = ItemStorage.Instance;
        _itemStorage.OnDataChanged += Refresh;
        Refresh(_itemStorage.CurrentCategory);
    }

    protected virtual void Refresh(EItemType currentCategory)
    {
        // 아이템 목록 업데이트
        List<InventoryItem> itemList = _itemStorage.GetStoredItemList(currentCategory);

        if (_isFixed)
        {
            UI_Category.Refresh(_fixedItemType, _fixedMainCategory);
        }
        else
        {
            // 아이템 카테고리 업데이트
            UI_Category.Refresh(currentCategory, _itemStorage.CurrentMainCategory);
        }
        // 아이템 슬롯 업데이트
        for (int i = 0; i < _itemSlotList.Count; i++)
        {
            _itemSlotList[i].Deselect();

            // 아이템 개수만큼 아이템 슬롯 활성화, 업데이트
            if (i < itemList.Count)
            {
                _itemSlotList[i].gameObject.SetActive(true);
                _itemSlotList[i].Refresh(itemList[i]);
            }
            else
            {
                // 나머지 아이템 슬롯 비활성화
                _itemSlotList[i].gameObject.SetActive(false);
            }
        }


        // 선택된 슬롯 업데이트
        if (_itemStorage.SelectedItemIndex == -1)
        {
            _selectedSlot = null;
        }
        else
        {
            _selectedSlot = _itemSlotList[_itemStorage.SelectedItemIndex];
            _selectedSlot.Select();
        }

        // 장착 슬롯 업데이트
        EquipmentSlotRefresh(currentCategory);
        
        if (_selectedSlot == null || !_selectedSlot.Item.IsEquipped)
        {
            EquipmentSlot.Deselect();
        }
        else
        {
            EquipmentSlot.Select();
        }

        // 선택된 아이템 이름 텍스트 업데이트
        if (_itemStorage.SelectedItemIndex == -1)
        {
            ItemNameText.text = "";
        }
        else
        {
            ItemNameText.text = _selectedSlot.Item.Item.Name;
        }
    }

    protected virtual void EquipmentSlotRefresh(EItemType currentCategory)
    {
        InventoryItem EquippedItem = _itemStorage.GetEquppedItem(currentCategory);
        EquipmentSlot.Refresh(EquippedItem);
    }

    public void Confirm()
    {
        if (_selectedSlot == null)
        {
            return;
        }
        
        InventoryItem selectedITem = _selectedSlot.Item;
        if (selectedITem.IsEquipped)
        {
            _itemStorage.UnEquipItem(selectedITem);
        }
        else
        {
            _itemStorage.EquipItem(selectedITem);
        }
    }

    // public void Show()
    // {
    //     gameObject.SetActive(true);
    // }
    //
    // public void Close()
    // {
    //     gameObject.SetActive(false);
    // }

    private void OnDestroy()
    {
        _itemStorage.OnDataChanged -= Refresh;
    }
}
