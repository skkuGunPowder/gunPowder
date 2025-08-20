using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class Shop : DontDestroySingleton<Shop>
{
    public Player_Preview Player_Preview;
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

    public async Task BuyItem(ECurrencyType currencyType)
    {
        int price = 0;

        if (currencyType == ECurrencyType.Diamond)
        {
            price = _selectedItem.DiamondPrice;
        }
        else
        {
            price = _selectedItem.GoldPrice;
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

        Result buyResult = await _repo.BuyItem(_selectedItem);
        if (!buyResult.IsSuccess)
        {
            Debug.LogError(buyResult.Message);
            CurrencyManager.Instance.AddCurrency(currencyType, price);
            return;
        }

        ItemStorage.Instance.AddItem(_selectedItem.ID);
        OnShopItemChanged?.Invoke(_shopItemDict, _selectedItem.ItemInfo.ItemType);
    }

    public void SelectItem(ShopItem item)
    {
        _selectedItem = item;
        Player_Preview.EquipItem(item);
        OnShopItemChanged?.Invoke(_shopItemDict, item.ItemInfo.ItemType);
    }

    public ShopItem GetSelectedItem()
    {
        if (_selectedItem == null)
        {
            Debug.LogWarning("선택된 아이템이 없습니다.");
            return null;
        }

        return _selectedItem;
    }

    public Dictionary<EItemType, List<ShopItem>> GetShopItemDict()
    {
        return _shopItemDict;
    }
}
