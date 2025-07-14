using UnityEngine;

public class Item
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public Sprite Image { get; private set; }
    public EEquipmentSlot EquipmentSlot { get; private set; }
    public bool IsEquipped { get; private set; }

    public Item(ItemDTO itemDTO)
    {
        // TODO
        // 유효성 검사하기

        ID = itemDTO.ID;
        Name = itemDTO.Name;
        Image = itemDTO.Image;
        EquipmentSlot = itemDTO.EquipmentSlot;
        IsEquipped = itemDTO.IsEquipped;
    }

    public Item(Sprite image, string id, string name, EEquipmentSlot equipmentSlot, bool isEquipped)
    {
        // TODO
        // 유효성 검사하기

        ID = id;
        Name = name;
        Image = image;
        EquipmentSlot = equipmentSlot;
        IsEquipped = isEquipped;
    }

    public void Select()
    {
        if (IsEquipped == true)
        {
            IsEquipped = false;
        }
        else
        {
            IsEquipped = true;
        }
    }
}
