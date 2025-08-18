using System.Collections.Generic;
using UnityEngine;

public class UI_ShopMainCategorySlot : MonoBehaviour
{
    public EShopMainCategory MainCategory;
    public List<EItemType> SubCategoryList;
    

    public void OnClick()
    {
        UI_Shop.Instance.SelectMainCategory(MainCategory);
    }
}
