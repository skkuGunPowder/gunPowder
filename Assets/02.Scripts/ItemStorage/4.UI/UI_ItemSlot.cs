using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour
{
    public ItemDTO Item;
    public Image Image;
    public Image EquippedIcon;


    public void Refresh(ItemDTO item)
    {
        Item = item;

        Image.sprite = Item.Image;
        if (Item.IsEquipped)
        {
            EquippedIcon.gameObject.SetActive(true);
        }
        else
        {
            EquippedIcon.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (Item == null)
        {
            Debug.LogError("아이템이 슬롯에 할당되지 않았습니다.");
            return;
        }

        if (EquippedIcon.gameObject.activeInHierarchy)
        {
            EquippedIcon.gameObject.SetActive(false);
        }
        else
        {
            EquippedIcon.gameObject.SetActive(true);
        }

        ItemStorage.Instance.SelectItem(Item);
    }
}
