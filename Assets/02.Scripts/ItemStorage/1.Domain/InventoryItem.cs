using System;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;


[FirestoreData]
public class InventoryItem
{
    [FirestoreProperty] public string ID { get; private set; }
    [FirestoreProperty] public bool IsEquipped { get; private set; }

    public ItemDTO Item;
    public Sprite Image;

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
