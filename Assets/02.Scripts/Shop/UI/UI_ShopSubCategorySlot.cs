using TMPro;
using UnityEngine;

public class UI_ShopSubCategorySlot : MonoBehaviour
{
    public TextMeshProUGUI SubCategoryText;
    private EItemType SubCategory;

    public void Refresh(EItemType itemType)
    {
        SubCategory = itemType;
        SubCategoryText.text = itemType.ToString();
    }

    public void OnClick()
    {
        UI_Shop.Instance.SelectSubCategory(SubCategory);
    }
}
