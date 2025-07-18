using System;
using UnityEngine;


public class ItemDTO
{
    public readonly string ID;
    public readonly EItemType ItemType;
    public readonly string Name;
    public readonly string Explanation;
    public readonly string ImageAddress;

    public readonly Sprite Image;


    public ItemDTO(Item item)
    {
        ID = item.ID;
        ItemType = item.ItemType;
        Name = item.Name;
        Explanation = item.Explanation;
        ImageAddress = item.ImageAddress;
        Image = item.Image;
    }
}
