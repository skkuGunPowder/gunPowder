using UnityEngine;
using System;
using System.Collections.Generic;


public class Shop : DontDestroySingleton<Shop>
{
    private Dictionary<EItemType, List<ShopItem>> _shopItemDict;

    private ShopItem _selectedItem;

    private ShopRepository _repo;

    public event Action<Dictionary<EItemType, List<ShopItem>>, EItemType> OnShopItemChanged;


    protected override void Awake()
    {
        base.Awake();

        _repo = new ShopRepository();
        _repo.OnLoadComplete += LoadShopItemData;
    }

    private void LoadShopItemData(Dictionary<EItemType, List<ShopItem>> shopItemDict)
    {
        Debug.LogWarning("상점 아이템 세팅");
        _shopItemDict = shopItemDict;
        OnShopItemChanged?.Invoke(_shopItemDict, EItemType.Event);
    }

    public async void BuyItem(ShopItem selectedItem, ECurrencyType currencyType)
    {
        int price = 0;

        if (currencyType == ECurrencyType.Diamond)
        {
            price = selectedItem.DiamondPrice;
        }
        else
        {
            price = selectedItem.GoldPrice;
        }

        if (price < 0)
        {
            throw new Exception("아이템 가격이 유효하지 않습니다!");
        }

        Result currencyResult = CurrencyManager.Instance.SubtractCurrency(currencyType, price);
        if (!currencyResult.IsSuccess)
        {
            Debug.LogError(currencyResult.Message);
            return;
        }

        Result buyResult = await _repo.BuyItem(selectedItem);
        if (!buyResult.IsSuccess)
        {
            Debug.LogError(buyResult.Message);
            CurrencyManager.Instance.AddCurrency(currencyType, price);
            return;
        }

        ItemStorage.Instance.AddItem(selectedItem.ID);
        OnShopItemChanged?.Invoke(_shopItemDict, selectedItem.ItemInfo.ItemType);
    }
}
