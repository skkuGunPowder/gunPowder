using System;
using UnityEngine;

[Serializable]
public class Item
{
    public string ID;
    public string Name;
    public Sprite Image;
    public EEquipmentSlot EquipmentSlot;
    public bool IsEquipped;

    public Item()
    {
        
    }

    public Item(ItemDTO itemDTO)
    {
        ID = itemDTO.ID;
        Name = itemDTO.Name;
        Image = itemDTO.Image;
        EquipmentSlot = itemDTO.EquipmentSlot;
        IsEquipped = itemDTO.IsEquipped;
    }

    public Item(Sprite image, string id, string name, EEquipmentSlot equipmentSlot, bool isEquipped)
    {
        if (image == null)
        {
            throw new Exception("아이콘 이미지가 없습니다.");
        }

        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("ID가 비어있습니다.");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("아이템 이름이 비어있습니다.");
        }

        ID = id;
        Name = name;
        Image = image;
        EquipmentSlot = equipmentSlot;
        IsEquipped = isEquipped;
    }

    public void Equip()
    {
        IsEquipped = true;
    }

    public void UnEquip()
    {
        IsEquipped = false;
    }
}
