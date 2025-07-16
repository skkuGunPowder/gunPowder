using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using Unity.VisualScripting;
using UnityEngine;


public class ItemStorageRepo
{
    /// <Test>
    private const string USER_ID = "USER_01";
    /// </Test>

    private const string SAVE_KEY = nameof(This);

    public async void SaveItemStorage(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        ItemStorageSaveData saveData = new ItemStorageSaveData(itemDict);
        string json = JsonUtility.ToJson(saveData);
        Debug.LogWarning(json);
        PlayerPrefs.SetString(SAVE_KEY, json);

        // TODO
        // DIctionary<EEquipmentSlot, List<item>> => Dictionary<string, object> 형태로 변환하기
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("ItemStores").Document(USER_ID);
        try
        {
            await docRef.SetAsync(itemDict);
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async Task<Dictionary<EEquipmentSlot, List<Item>>> LoadItemStorage()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("저장된 아이템 정보가 없습니다.");
            return null;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        ItemStorageSaveData saveData = JsonUtility.FromJson<ItemStorageSaveData>(json);

        // TODO
        // Dictionary<string, object> => DIctionary<EEquipmentSlot, List<item>>형태로 변환하기
        CollectionReference itemStorageRef = FirebaseManager.Instance.DB.Collection("ItemStores");

        try
        {
            DocumentSnapshot document = await itemStorageRef.Document(USER_ID).GetSnapshotAsync();
            if (document.Exists)
            {
                Debug.Log("저장된 데이터 불러오기 성공!");
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }


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
