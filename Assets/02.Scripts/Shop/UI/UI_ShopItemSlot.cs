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
        GoldText.transform.parent.gameObject.SetActive(true);
        DiamondText.gameObject.SetActive(true);

        if (item.GoldPrice == -1)
        {
            GoldText.transform.parent.gameObject.SetActive(false);
        }

        if (item.DiamondPrice == -1)
        {
            DiamondText.gameObject.SetActive(false);
        }

        GoldText.text = $"{item.GoldPrice}";
        DiamondText.text = $"{item.DiamondPrice}";
        IconImage.sprite = item.ItemInfo.Image;
    }
}
