using System;
using UnityEngine;


[Serializable]
public class Item
{
    public readonly string ID;
    public readonly EItemType ItemType;
    public readonly string Name;
    public readonly string Explanation;
    public readonly string ImageAddress;

    public readonly Sprite Image;


    public Item()
    {

    }
    
    public Item(string id, EItemType itemType, string name, string explanation, string imageAddress, Sprite image)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("ID가 비어있습니다.");
        }

        if (itemType == EItemType.None)
        {
            throw new Exception("아이템 타입이 올바르지 않습니다.");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("아이템 이름이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(explanation))
        {
            throw new Exception("아이템 설명이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(imageAddress))
        {
            throw new Exception("아이콘 이미지 주소가 없습니다.");
        }

        ID = id;
        ItemType = itemType;
        Name = name;
        Explanation = explanation;
        ImageAddress = imageAddress;

        Image = image;
    }
}
