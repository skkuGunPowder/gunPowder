using UnityEngine;


public class ItemDTO
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public Sprite Image { get; private set; }
    public EEquipmentSlot EquipmentSlot { get; private set; }
    public bool IsEquipped { get; private set; }

    public ItemDTO(Item item)
    {
        ID = item.ID;
        Name = item.Name;
        Image = item.Image;
        EquipmentSlot = item.EquipmentSlot;
        IsEquipped = item.IsEquipped;
    }

    public ItemDTO(Sprite image, string id, string name, EEquipmentSlot equipmentSlot, bool isEquipped)
    {
        ID = id;
        Name = name;
        Image = image;
        EquipmentSlot = equipmentSlot;
        IsEquipped = isEquipped;
    }
}
