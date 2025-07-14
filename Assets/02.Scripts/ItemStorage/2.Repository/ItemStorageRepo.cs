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
}


[Serializable]
public class ItemStorageSaveData
{
    [SerializeField] public List<Item> HeadItemList;
    [SerializeField] public List<Item> FaceItemList;
    [SerializeField] public List<Item> ChestItemList;
    [SerializeField] public List<Item> WeaponItemList;
    [SerializeField] public List<Item> BackItemList;


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
