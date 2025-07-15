using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemStorage : MonoBehaviour
{
    public ItemDTO SelectedItem;
    public TextMeshProUGUI ItemNameText;
    public List<UI_ItemSlot> ItemSlotList;

    private ItemStorage _itemStorage;
    private UI_ItemSlot _selectedItemSlot;

    private void Start()
    {
        _itemStorage = ItemStorage.Instance;

        // 이벤트 구독
        _itemStorage.OnDataChange += Refresh;

        Refresh(_itemStorage._storedItemDict[_itemStorage.CurrentCategory].ConvertAll(x => new ItemDTO(x)));
    }

    private void Refresh(List<ItemDTO> itemList)
    {
        // 전체 아이템 슬롯 순회
        for (int i = 0; i < ItemSlotList.Count; i++)
        {
            // 아이템 개수만큼 아이템 슬롯 활성화
            if (i < itemList.Count)
            {
                ItemSlotList[i].gameObject.SetActive(true);
                ItemSlotList[i].Refresh(itemList[i]);
            }
            else
            {
                // 나머지 아이템 슬롯 비활성화
                ItemSlotList[i].gameObject.SetActive(false);
            }

        }

        if (SelectedItem == null)
        {
            ItemNameText.text = "";
        }
        else
        {
            ItemNameText.text = SelectedItem.Name;
        }
    }
}
