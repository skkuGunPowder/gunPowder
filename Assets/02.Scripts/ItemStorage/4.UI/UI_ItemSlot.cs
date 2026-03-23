using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, ISelectable, IPointerClickHandler
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

        // 폭탄: 좌클릭 단일클릭으로 메인 폭탄 장착
        if (Item.Item.ItemType == EItemType.Bomb)
        {
            ItemStorage.Instance.SelectItem(Item);
            ItemStorage.Instance.EquipItem(Item);
            return;
        }

        // 기타 카테고리: 더블클릭으로 착용
        ItemStorage.Instance.SelectItem(Item);

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null) return;
        if (Item.Item.ItemType != EItemType.Bomb) return;
        if (eventData.button != PointerEventData.InputButton.Right) return;

        // 우클릭: SubBomb 착용/해제 토글
        InventoryItem currentSubBomb = ItemStorage.Instance.GetEquippedSubBomb();
        if (currentSubBomb != null && currentSubBomb.ID == Item.ID)
            ItemStorage.Instance.UnEquipSubBomb();
        else
            ItemStorage.Instance.EquipAsSubBomb(Item);
    }
}
