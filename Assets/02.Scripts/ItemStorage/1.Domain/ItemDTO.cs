using System;
using UnityEngine;

[Serializable]
public class ItemDTO
{
    public string ID;
    public string Name;
    public string Description;
    public Sprite Image;
    public string ImageAddress;
    public EEquipmentSlot EquipmentSlot;
    public bool IsEquipped;

    public ItemDTO()
    {
        
    }

    
    public ItemDTO(Item item)
    {
        ID = item.ID;
        Name = item.Name;
        Description = item.Description;
        Image = item.Image;
        ImageAddress = item.ImageAddress;
        EquipmentSlot = item.EquipmentSlot;
        IsEquipped = item.IsEquipped;
    }

    public ItemDTO(string id, string name, string description, Sprite image, string imageAddress, EEquipmentSlot equipmentSlot, bool isEquipped)
    {
        ID = id;
        Name = name;
        Description = description;
        Image = image;
        ImageAddress = imageAddress;
        EquipmentSlot = equipmentSlot;
        IsEquipped = isEquipped;
    }
}
