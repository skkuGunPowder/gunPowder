using System.Collections.Generic;
using UnityEngine;

public class UI_Shop : MonoBehaviour
{
    public UI_MainEventPage MainEventPage;
    public UI_ShoppingPage ShoppingPage;

    private EItemType _currentCategory;

    private void Awake()
    {
        Shop.Instance.OnShopItemChanged += Refresh;

        ShowMainPage();
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

    public void Refresh(Dictionary<EItemType, List<ShopItem>> shopItemDcit)
    {
        if (MainEventPage.isActiveAndEnabled)
        {
            MainEventPage.Refresh();
            return;
        }

        if (ShoppingPage.isActiveAndEnabled)
        {
            ShoppingPage.Refresh(shopItemDcit[_currentCategory]);
        }
    }
}
