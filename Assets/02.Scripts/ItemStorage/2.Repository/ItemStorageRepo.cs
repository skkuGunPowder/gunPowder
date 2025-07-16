using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ItemStorageRepo
{
    private const string SAVE_KEY = nameof(This);

    public void SaveItemStorage(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        ItemStorageSaveData saveData = new ItemStorageSaveData(itemDict);
        string json = JsonUtility.ToJson(saveData);
        Debug.LogWarning(json);
        PlayerPrefs.SetString(SAVE_KEY, json);

        // TODO
        // firebase DB랑 연동하기
    }

    public Dictionary<EEquipmentSlot, List<Item>> LoadItemStorage()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("저장된 아이템 정보가 없습니다.");
            return null;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        ItemStorageSaveData saveData = JsonUtility.FromJson<ItemStorageSaveData>(json);

        // TODO
        // firebase DB랑 연동하기

        // return saveData.ToDictionary();
        return null;
    }

    public Dictionary<EEquipmentSlot, Item> LoadInventory()
    {
        // TODO
        return null;
    }

    public void SaveInventory()
    {
        // TODO
    }
}



[Serializable]
public class ItemStorageSaveData
{
    public List<Item> HeadItemList;
    public List<Item> FaceItemList;
    public List<Item> ChestItemList;
    public List<Item> WeaponItemList;
    public List<Item> BackItemList;


    public ItemStorageSaveData(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        foreach (var kvp in itemDict)
        {
            switch (kvp.Key)
            {
                case EEquipmentSlot.Head:
                    HeadItemList = kvp.Value;
                    break;

                case EEquipmentSlot.Face:
                    FaceItemList = kvp.Value;
                    break;

                case EEquipmentSlot.Chest:
                    ChestItemList = kvp.Value;
                    break;

                case EEquipmentSlot.Weapon:
                    WeaponItemList = kvp.Value;
                    break;

                case EEquipmentSlot.Back:
                    BackItemList = kvp.Value;
                    break;

                default:
                    break;
            }
        }
    }

    public Dictionary<EEquipmentSlot, List<Item>> ToDictionary()
    {
        Dictionary<EEquipmentSlot, List<Item>> itemDict = new Dictionary<EEquipmentSlot, List<Item>>();

        itemDict.Add(EEquipmentSlot.Head, HeadItemList);
        itemDict.Add(EEquipmentSlot.Face, FaceItemList);
        itemDict.Add(EEquipmentSlot.Chest, ChestItemList);
        itemDict.Add(EEquipmentSlot.Weapon, WeaponItemList);
        itemDict.Add(EEquipmentSlot.Back, BackItemList);

        return itemDict;
    }
}
