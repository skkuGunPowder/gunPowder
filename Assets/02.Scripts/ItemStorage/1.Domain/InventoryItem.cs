using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public class InventoryItem
{
    public readonly string ID;
    public readonly ItemDTO Item;
    public Sprite Image;
    public bool IsEquipped;


    public InventoryItem()
    {

    }

    public InventoryItem(ItemDTO itemDTO)
    {
        if (itemDTO == null)
        {
            throw new Exception("아이템 정보가 없습니다.");
        }

        ID = itemDTO.ID;
        Item = itemDTO;
        
        LoadImageAsync();
    }

    private async void LoadImageAsync()
    {
        Image = await Addressables.LoadAssetAsync<Sprite>(Item.ImageAddress).Task;
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
