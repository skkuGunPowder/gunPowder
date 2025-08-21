using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShoppingPage : MonoBehaviour
{
    [SerializeField] private List<UI_ShopItemSlot> _shopSlotList;

    [SerializeField] private UI_ShopItemSlot _shopSlotPrefab;
    [SerializeField] private Transform _gridTransform;

    [SerializeField] private Transform BuyGoldButton;
    [SerializeField] private Transform BuyDiamondButton;
    [SerializeField] private TextMeshProUGUI BuyGoldButtonText;
    [SerializeField] private TextMeshProUGUI BuyDiamondButtonText;

    private ShopItem _selectedItem;


    private void Awake()
    {
        _shopSlotList = new List<UI_ShopItemSlot>();
    }

    public void Refresh(List<ShopItem> shopItemList)
    {
        _selectedItem = Shop.Instance.GetSelectedItem();

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

        if (_selectedItem == null)
        {
            BuyGoldButton.gameObject.SetActive(false);
            BuyDiamondButton.gameObject.SetActive(false);
            return;
        }

        if (_selectedItem.GoldPrice > -1)
            {
                BuyGoldButton.gameObject.SetActive(true);
                BuyGoldButtonText.text = _selectedItem.GoldPrice.ToString("N0");
            }
            else
            {
                BuyGoldButton.gameObject.SetActive(false);
            }

        if (_selectedItem.DiamondPrice > -1)
        {
            BuyDiamondButton.gameObject.SetActive(true);
            BuyDiamondButtonText.text = _selectedItem.DiamondPrice.ToString("N0");
        }
        else
        {
            BuyDiamondButton.gameObject.SetActive(false);
        }
    }

    public void OnClickBuyWithGold()
    {
        Shop.Instance.BuyItem(ECurrencyType.Gold);
    }

    public void OnClickBuyWithDiamon()
    {
        Shop.Instance.BuyItem(ECurrencyType.Diamond);
    }
}
