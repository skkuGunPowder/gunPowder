using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour
{
    public ItemDTO Item;
    public Image ItemIcon;
    public Image EquippedIcon;
    public Image SelectedIcon;

    private bool _isSelected = false;


    public void Refresh(ItemDTO item)
    {
        Item = item;
        ItemIcon.sprite = Item.Image;

        if (Item.IsEquipped)
        {
            EquippedIcon.gameObject.SetActive(true);
        }
        else
        {
            EquippedIcon.gameObject.SetActive(false);
        }

        if (_isSelected)
        {
            SelectedIcon.gameObject.SetActive(true);
        }
        else
        {
            SelectedIcon.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (Item == null)
        {
            Debug.LogError("아이템이 슬롯에 할당되지 않았습니다.");
            return;
        }

        _isSelected = true;
        ItemStorage.Instance.SelectItem(Item);
    }
}
