using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShopItemSlot : MonoBehaviour
{
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI DiamondText;
    public Image IconImage;

    public void Refresh(ShopItem item)
    {
        GoldText.text = $"{item.GoldPrice}";
        DiamondText.text = $"{item.DiamondPrice}";
        IconImage.sprite = item.ItemInfo.Image;
    }
}
