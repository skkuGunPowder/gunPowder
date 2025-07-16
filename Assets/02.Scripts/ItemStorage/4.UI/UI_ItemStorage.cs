using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemStorage : MonoBehaviour
{
    public TextMeshProUGUI ItemNameText;
    public UI_EquipmentSlot EquipmentSlot;

    [SerializeField] private List<UI_ItemSlot> _itemSlotList;
    [SerializeField] private List<UI_Category> _categorieList;

    private ItemStorage _itemStorage;

    private UI_ItemSlot _selectedSlot;


    private void Start()
    {
        _itemStorage = ItemStorage.Instance;
        _itemStorage.OnDataChanged += Refresh;

        Refresh(_itemStorage.CurrentCategory);
    }

    private void Refresh(EEquipmentSlot currentCategory)
    {
        List<ItemDTO> itemList = _itemStorage.GetStoredItemList(currentCategory);

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

        // 카테고리 슬롯 업데이트
        foreach (UI_Category category in _categorieList)
        {
            if (category.Category == currentCategory)
            {
                category.Select();
                continue;
            }

            category.Deselect();
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
        ItemDTO EquippedItem = _itemStorage.GetEquppedItem(currentCategory);
        EquipmentSlot.Refresh(EquippedItem);
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
            ItemNameText.text = _selectedSlot.Item.Name;
        }
    }
}
