using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_Shop : Singleton<UI_Shop>
{
    public TextMeshProUGUI PlayerGoldText;
    public TextMeshProUGUI PlayerDiamondText;

    public UI_MainEventPage MainEventPage;
    public UI_ShoppingPage ShoppingPage;

    public Image SubCategoryTab;
    public Image MainCategoryTab;

    public List<Sprite> SubTabImageList;
    public List<Sprite> MainTabImageList;

    public List<UI_ShopMainCategorySlot> MainCategorySlotList;
    public List<UI_ShopSubCategorySlot> SubCategorySlotList;

    private Dictionary<EItemType, List<ShopItem>> _shopItemDict;

    private ShopItem _selectedItem;


    protected override void Awake()
    {
        CurrencyManager.Instance.OnDataChanged += RefreshPlayerCurrency;
        Shop.Instance.OnShopItemChanged += Refresh;
        SelectMainCategory(EShopMainCategory.Event);

        RefreshPlayerCurrency(CurrencyManager.Instance.PlayerGold.GetAmount(), CurrencyManager.Instance.PlayerGold.GetAmount());
    }

    public void ShowMainPage()
    {
        MainEventPage.gameObject.SetActive(true);
        ShoppingPage.gameObject.SetActive(false);
    }

    public void ShowShoppingPage()
    {
        MainEventPage.gameObject.SetActive(false);
        ShoppingPage.gameObject.SetActive(true);
    }

    public void SelectMainCategory(EShopMainCategory selectedMainCategory)
    {
        MainCategoryTab.sprite = MainTabImageList[(int)selectedMainCategory];
        SubCategoryTab.sprite = SubTabImageList[(int)selectedMainCategory];

        if (selectedMainCategory == EShopMainCategory.Event)
        {
            ShowMainPage();
        }
        else
        {
            ShowShoppingPage();
        }

        List<EItemType> subCategoryList = null;

        foreach (var mainCategorySlot in MainCategorySlotList)
        {
            if (mainCategorySlot.MainCategory == selectedMainCategory)
            {
                subCategoryList = mainCategorySlot.SubCategoryList;
            }
        }

        for (int i = 0; i < SubCategorySlotList.Count; i++)
            {
                if (subCategoryList.Count > i)
                {
                    SubCategorySlotList[i].gameObject.SetActive(true);
                    SubCategorySlotList[i].Refresh(subCategoryList[i]);
                    continue;
                }
                SubCategorySlotList[i].gameObject.SetActive(false);
            }

        SelectSubCategory(subCategoryList[0]);
    }

    public void SelectSubCategory(EItemType itemType)
    {
        Refresh(_shopItemDict, itemType);
    }

    public void RefreshPlayerCurrency(int goldAmount, int diamondAmount)
    {
        PlayerGoldText.text = $"{goldAmount}";
        PlayerDiamondText.text = $"{diamondAmount}";
    }

    public void Refresh(Dictionary<EItemType, List<ShopItem>> shopItemDcit, EItemType currentCategory)
    {
        _shopItemDict = shopItemDcit;

        MainEventPage.Refresh();
        ShoppingPage.Refresh(shopItemDcit[currentCategory]);
    }
}
