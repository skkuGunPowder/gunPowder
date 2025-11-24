using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, ISelectable
{
    public InventoryItem Item;
    
    public Image ItemIcon;
    public Image SelectedIcon;

    private int _clickCount = 0;
    private float _clickTimer = 0f;


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

        // 더블클릭 시 장착
        _clickCount++;
        if (_clickCount == 1)
        {
            _clickTimer = Time.time;
        }
        else if (_clickCount == 2 && Time.time - _clickTimer <= 0.5f)
        {
            ItemStorage.Instance.EquipItem(Item);
            _clickCount = 0;
        }
        else
        {
            _clickCount = 1;
            _clickTimer = Time.time;
        }

    }
}
