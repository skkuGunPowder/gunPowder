using System;
using UnityEngine;

[Serializable]
public class Item
{
    public string ID;
    public string Name;
    public string Description;
    public Sprite Image;
    public string ImageAddress;
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
        ImageAddress = itemDTO.ImageAddress;
        Description = itemDTO.Description;
        EquipmentSlot = itemDTO.EquipmentSlot;
        IsEquipped = itemDTO.IsEquipped;
    }

    public Item(string id, string name, string description, Sprite image, string imageAddress, EEquipmentSlot equipmentSlot, bool isEquipped)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("ID가 비어있습니다.");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("아이템 이름이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(description))
        {
            throw new Exception("아이템 설명이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(imageAddress))
        {
            throw new Exception("아이콘 이미지 주소가 없습니다.");
        }

        ID = id;
        Name = name;
        Description = description;
        Image = image;
        ImageAddress = imageAddress;
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
