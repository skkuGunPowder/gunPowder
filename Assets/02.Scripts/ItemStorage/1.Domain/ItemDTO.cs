using System;
using UnityEngine;

[Serializable]
public class ItemDTO
{
    public string ID;
    public string Name;
    public Sprite Image;
    public EEquipmentSlot EquipmentSlot;
    public bool IsEquipped;

    public ItemDTO()
    {
        
    }

    
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
