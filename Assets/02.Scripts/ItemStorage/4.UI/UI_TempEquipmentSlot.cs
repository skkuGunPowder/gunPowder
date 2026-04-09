public class UI_TempEquipmentSlot : UI_EquipmentSlot
{
    public bool Selected = false;
    
    public override void Refresh(InventoryItem item)
    {
        Item = item;

        if (item == null)
        {
            ItemIcon.gameObject.SetActive(false);
            Selected = false;
            return;
        }

        gameObject.SetActive(true);
        ItemIcon.gameObject.SetActive(true);
        ItemIcon.sprite = Item.Image;
        Selected = true;
    }

    protected override void DeSelectAction()
    {
    }

    protected override void SelectAction()
    {
    }
}
