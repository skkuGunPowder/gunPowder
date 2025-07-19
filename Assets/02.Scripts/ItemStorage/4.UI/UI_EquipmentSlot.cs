using UnityEngine;
using UnityEngine.UI;

public class UI_EquipmentSlot : MonoBehaviour, ISelectable
{
    public InventoryItem Item;

    public Image ItemIcon;
    public Image EquippedIcon;
    public Image SelectedIcon;


    public void Refresh(InventoryItem item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        Item = item;
        ItemIcon.sprite = Item.Image;
    }

    public void Select()
    {
        SelectedIcon.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        SelectedIcon.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if (Item == null)
        {
            throw new System.Exception("아이템이 슬롯에 할당되지 않았습니다.");
        }

        ItemStorage.Instance.SelectItem(Item);
    }
}
