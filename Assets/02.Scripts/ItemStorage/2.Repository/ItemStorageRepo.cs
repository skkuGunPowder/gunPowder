using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using UnityEngine;

public class ItemStorageRepo
{
    private const string USER_ID = "USER_01";

    public event Action<Dictionary<EItemType, List<InventoryItem>>> OnItemStorageLoaded;
    public event Action<Dictionary<EItemType, InventoryItem>> OnInventoryLoaded;


    public async void SaveItemStorage(Dictionary<EItemType, List<InventoryItem>> itemDict)
    {
        // FireStore에 저장가능한 형태로 데이터 변환
        Dictionary<string, List<SerializableItem>> saveData = ConvertToSaveData(itemDict);

        // 데이터 저장
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("ItemStorage").Document(USER_ID);
        try
        {
            await docRef.SetAsync(new Dictionary<string, object> { { "InStorage", saveData } });
            
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async void LoadItemStorage()
    {
        // 데이터 로드
        CollectionReference itemStorageRef = FirebaseManager.Instance.DB.Collection("ItemStorage");
        try
        {
            DocumentSnapshot document = await itemStorageRef.Document(USER_ID).GetSnapshotAsync();

            if (document.Exists)
            {
                var rawItemStorageData = document.GetValue<Dictionary<string, object>>("InStorage");
                var itemDict = new Dictionary<EItemType, List<InventoryItem>>();
                
                // 아이템 딕셔너리에 데이터 할당
                foreach (var kvp in rawItemStorageData)
                {
                    string key = kvp.Key;
                    if (!Enum.TryParse(key, out EItemType slot))
                    {
                        continue;
                    }
                    
                    // 아이템 리스트에 데이터 할당
                    var itemList = new List<InventoryItem>();
                    foreach (var obj in (List<object>)kvp.Value)
                    {
                        InventoryItem item = ConvertToItem((Dictionary<string, object>)obj);
                        itemList.Add(item);
                    }

                    itemDict[slot] = itemList;
                }
                Debug.Log("보유중인 아이템 불러오기 성공!");
                OnItemStorageLoaded?.Invoke(itemDict);
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async void LoadInventory()
    {
        // 데이터 로드
        CollectionReference inventoryRef = FirebaseManager.Instance.DB.Collection("Inventory");
        try
        {
            DocumentSnapshot document = await inventoryRef.Document(USER_ID).GetSnapshotAsync();
            if (document.Exists)
            {
                var rawInventoryData = document.GetValue<Dictionary<string, object>>("Equipments");
                var equippedItemDict = new Dictionary<EItemType, InventoryItem>();

                // 장착된 아이템 딕셔너리에 데이터 할당
                foreach (var kvp in rawInventoryData)
                {
                    string key = kvp.Key;

                    // key -> EItemType 파싱
                    if (!Enum.TryParse(key, out EItemType slot))
                    {
                        throw new Exception($"{key} 아이템 타입을 파싱하는데 실패하였습니다");
                    }

                    // 아이템 데이터 있을 시 Item 객체로 변환하여 할당
                    equippedItemDict[slot] = ConvertToItem((Dictionary<string, object>)kvp.Value);
                }
                Debug.Log("장착된 아이템 불러오기 성공!");
                OnInventoryLoaded?.Invoke(equippedItemDict);
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Inventory 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async void SaveInventory(Dictionary<EItemType, InventoryItem> equippedItemDict)
    {
        // FireStore에 저장가능한 형태로 데이터 변환
        Dictionary<string, SerializableItem> saveData = ConvertInventoryData(equippedItemDict);

        // 데이터 저장
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("Inventory").Document(USER_ID);
        try
        {
            await docRef.SetAsync(new Dictionary<string, object> { { "Equipments", saveData } });
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public Dictionary<string, List<SerializableItem>> ConvertToSaveData(Dictionary<EItemType, List<InventoryItem>> itemDict)
    {
        // 아이템 보관함 데이터 => 저장가능한 데이터로 변환하는 메소드

        var saveData = new Dictionary<string, List<SerializableItem>>();
        foreach (var kvp in itemDict)
        {
            List<SerializableItem> serializableItemList = new List<SerializableItem>();

            foreach (var item in kvp.Value)
            {
                serializableItemList.Add(new SerializableItem(item));
            }

            string slotKey = kvp.Key.ToString();
            saveData[slotKey] = serializableItemList;
        }

        return saveData;
    }

    public Dictionary<string, SerializableItem> ConvertInventoryData(Dictionary<EItemType, InventoryItem> equippedItemDict)
    {
        // 인벤토리 데이터 => 저장가능한 데이터로 변환하는 메소드

        var saveData = new Dictionary<string, SerializableItem>();
        foreach (var kvp in equippedItemDict)
        {
            string slotKey = kvp.Key.ToString();

            if (kvp.Value == null)
            {
                // 아이템 데이터 없을 시 null로 변환
                saveData[slotKey] = null;
            }
            else
            {
                // 아이템 데이터 있을 시 저장가능한 형태로 변환
                saveData[slotKey] = new SerializableItem(kvp.Value);
            }
        }

        return saveData;
    }


    private InventoryItem ConvertToItem(Dictionary<string, object> dict)
    {
        // 저장된 데이터 -> Item 객체로 변환하는 메소드
        if (dict == null)
        {
            return null;
        }

        // InventoryItem객체로 변환
            ItemDTO item = ItemDatabase.Instance.GetItem((string)dict["ID"]);
        if (item == null)
        {
            Debug.LogError($"{(string)dict["ID"]} : null");
            return null;
        }

        InventoryItem inventoryItem = new InventoryItem(item);
        if ((bool)dict["IsEquipped"])
        {
            inventoryItem.Equip();
        }
        else
        {
            inventoryItem.UnEquip();
        }

        return inventoryItem;
    }
}


[FirestoreData]
public class SerializableItem
{
    [FirestoreProperty] public string ID { get; set; }
    [FirestoreProperty] public bool IsEquipped { get; set; }

    public SerializableItem()
    {

    }

    public SerializableItem(InventoryItem item)
    {
        if (string.IsNullOrEmpty(item.ID))
        {
            throw new Exception("ID가 비어있습니다.");
        }

        ID = item.ID;
        IsEquipped = item.IsEquipped;
    }
}
