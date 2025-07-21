using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, ISelectable
{
    public InventoryItem Item;
    
    public Image ItemIcon;
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

        if (Item.IsEquipped)
        {
            gameObject.SetActive(false);
        }
        else
        {
           gameObject.SetActive(true);
        }
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
