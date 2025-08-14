using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShopItemSlot : MonoBehaviour, ISelectable
{
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI DiamondText;
    public Image IconImage;
    public Image SelectedImage;

    private ShopItem _shopItem;
    private bool _isSelected = false;

    public void Refresh(ShopItem item)
    {
        _shopItem = item;

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

    public void Select()
    {
        _isSelected = true;
        SelectedImage.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        _isSelected = false;
        SelectedImage.gameObject.SetActive(false);
    }


    public void OnClick()
    {
        if (_isSelected)
        {
            Deselect();
            Shop.Instance.SelectItem(null);
        }
        else
        {
            Select();
            Shop.Instance.SelectItem(_shopItem);
        }
    }
}
