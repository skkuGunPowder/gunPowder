using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BombSlot : UI_ItemSlot, IPointerClickHandler
{
    [SerializeField] private GameObject _subOutline;
    [SerializeField] private GameObject _mainOutline;
    [SerializeField] private Button  _button;
    public override void Refresh(InventoryItem item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        Item = item;
        ItemIcon.sprite = Item.Image;
        
        InventoryItem subBomb = ItemStorage.Instance.GetEquippedSubBomb();
        bool isSubBombSlot = subBomb != null && subBomb.ID == Item.ID;
        
        if (isSubBombSlot)
        {
            SubEquipAction();
            return;
        }
        
        if (Item.IsEquipped)
        {
            EquipAction();
        }
        else
        {
            UnEquipAction();
        }
    }
    protected override void ClickAction()
    {
        if (Item == null)
        {
            throw new System.Exception("아이템이 슬롯에 할당되지 않았습니다.");
        }

        if (_subOutline.activeSelf)
        {
            return;
        }
        
        if (_mainOutline.activeSelf)
        {
            ItemStorage.Instance.UnEquipItem(Item);
            return;
        }
        
        ItemStorage.Instance.EquipItem(Item);
    }

    private void SubEquipAction()
    {
        _subOutline.SetActive(true);
        _mainOutline.SetActive(false);
        SelectedIcon.gameObject.SetActive(true);
        _button.interactable = false;
    }

    protected override void EquipAction()
    {
        _mainOutline.SetActive(true);   
        SelectedIcon.gameObject.SetActive(true);
        _subOutline.SetActive(false);
        _button.interactable = false;
    }

    protected override void UnEquipAction()
    {
        _mainOutline.SetActive(false);
        SelectedIcon.gameObject.SetActive(false);
        _subOutline.SetActive(false);
        _button.interactable = true;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null) 
        {
            return;
            
        }

        if (Item.Item.ItemType != EItemType.Bomb)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (_mainOutline.activeSelf)
        {
            return;
        }
        // 우클릭: SubBomb 착용/해제 토글
        InventoryItem currentSubBomb = ItemStorage.Instance.GetEquippedSubBomb();
        if (currentSubBomb != null && currentSubBomb.ID == Item.ID)
        {
            ItemStorage.Instance.UnEquipSubBomb();   
        }
        else
        {
            ItemStorage.Instance.EquipAsSubBomb(Item);   
        }
    }
}
