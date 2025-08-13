using System.Collections.Generic;
using UnityEngine;

public class UI_ShoppingPage : MonoBehaviour
{
    [SerializeField] private List<UI_ShopItemSlot> _shopSlotList;

    [SerializeField] private UI_ShopItemSlot _shopSlotPrefab;
    [SerializeField] private Transform _gridTransform;

    private void Awake()
    {
        _shopSlotList = new List<UI_ShopItemSlot>();
    }

    public void Refresh(List<ShopItem> shopItemList)
    {
        if (_shopSlotList.Count < shopItemList.Count)
        {
            for (int i = 0; i < shopItemList.Count - _shopSlotList.Count; i++)
            {
                var newShopSlot = Instantiate(_shopSlotPrefab, _gridTransform);
                _shopSlotList.Add(newShopSlot);
                newShopSlot.gameObject.SetActive(false);
            }
        }


        for (int i = 0; i < _shopSlotList.Count; i++)
            {
                if (shopItemList.Count > i)
                {
                    _shopSlotList[i].gameObject.SetActive(true);
                    _shopSlotList[i].Refresh(shopItemList[i]);
                    continue;
                }
                _shopSlotList[i].gameObject.SetActive(false);
            }
    }
}
