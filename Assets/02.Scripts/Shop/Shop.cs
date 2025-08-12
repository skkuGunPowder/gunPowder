using UnityEngine;
using System;
using System.Collections.Generic;


public class Shop : Singleton<Shop>
{
    private Dictionary<EItemType, List<ShopItem>> _shopItemDict;

    private ShopRepository _repo;

    public event Action<Dictionary<EItemType, List<ShopItem>>> OnShopItemChanged;

    protected override void Awake()
    {
        base.Awake();

        _repo = new ShopRepository();
        _repo.OnLoadComplete += LoadShopItemData;
    }

    private void LoadShopItemData(Dictionary<EItemType, List<ShopItem>> shopItemDict)
    {
        _shopItemDict = shopItemDict;
        OnShopItemChanged?.Invoke(_shopItemDict);
    }

    public async void BuyItem(ShopItem selectedItem, int amount)
    {

        if (selectedItem.GoldPrice > 0)
        {
            Result goldResult = CurrencyManager.Instance.SubtractGold(selectedItem.GoldPrice);
            if (!goldResult.IsSuccess)
            {
                Debug.LogError(goldResult.Message);
                return;
            }
        }

        if (selectedItem.DiamondPrice > 0)
        {
            // TODO
            // CurrencyManager.Instance.SubtractDiamond(selectedItem.DiamondPrice);
        }

        if (selectedItem.CashPrice > 0)
        {
            // TODO: 현금 결제 처리
        }

        Result result = await _repo.BuyItem(selectedItem, amount);
        if (!result.IsSuccess)
        {
            Debug.LogError(result.Message);
            CurrencyManager.Instance.AddGold(selectedItem.GoldPrice);
            return;
        }

        ItemStorage.Instance.AddItem(selectedItem.ID);
        OnShopItemChanged?.Invoke(_shopItemDict);
    }
}
