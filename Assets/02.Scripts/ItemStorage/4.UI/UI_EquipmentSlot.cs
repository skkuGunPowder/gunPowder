using UnityEngine;
using UnityEngine.UI;

public class UI_EquipmentSlot : MonoBehaviour, ISelectable
{
    public InventoryItem Item;

    public Image ItemIcon;
    public Image EquippedIcon;
    public Image SelectedIcon;


    public virtual void Refresh(InventoryItem item)
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
        SelectAction();
    }

    public void Deselect()
    {
        DeSelectAction();
    }

    protected virtual void SelectAction()
    {
        SelectedIcon.gameObject.SetActive(true);
    }
    protected virtual void DeSelectAction()
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
