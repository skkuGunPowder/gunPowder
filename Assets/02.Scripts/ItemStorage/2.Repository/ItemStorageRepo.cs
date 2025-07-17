using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemStorageRepo
{
    #if UNITY_EDITOR
    private const string USER_ID = "USER_01";
    #endif

    public event Action<Dictionary<EEquipmentSlot, List<Item>>> OnItemStorageLoaded;
    public event Action<Dictionary<EEquipmentSlot, Item>> OnInventoryLoaded;


    public async void SaveItemStorage(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("ItemStorage").Document(USER_ID);

        Dictionary<string, List<SerializableItem>> saveData = ConvertItemStorageData(itemDict);
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
        CollectionReference itemStorageRef = FirebaseManager.Instance.DB.Collection("ItemStorage");

        try
        {
            DocumentSnapshot document = await itemStorageRef.Document(USER_ID).GetSnapshotAsync();

            if (document.Exists)
            {
                var rawItemStorageData = document.GetValue<Dictionary<string, object>>("InStorage");

                var itemDict = new Dictionary<EEquipmentSlot, List<Item>>();

                foreach (var kvp in rawItemStorageData)
                {
                    string key = kvp.Key;
                    if (!Enum.TryParse(key, out EEquipmentSlot slot))
                    {
                        continue;
                    }

                    var itemList = new List<Item>();

                    foreach (var obj in kvp.Value as List<object>)
                    {
                        Item item = await ConvertToItemAsync(obj as Dictionary<string, object>);
                        itemList.Add(item);
                    }

                    itemDict[slot] = itemList;
                }
                Debug.Log("저장된 데이터 불러오기 성공!");
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
        CollectionReference inventoryRef = FirebaseManager.Instance.DB.Collection("Inventory");

        try
        {
            DocumentSnapshot document = await inventoryRef.Document(USER_ID).GetSnapshotAsync();

            if (document.Exists)
            {
                var rawInventoryData = document.GetValue<Dictionary<string, object>>("Equipments");
                var equippedItemDict = new Dictionary<EEquipmentSlot, Item>();

                foreach (var kvp in rawInventoryData)
                {
                    string key = kvp.Key;
                    if (!Enum.TryParse(key, out EEquipmentSlot slot))
                    {
                        continue;
                    }

                    if (kvp.Value == null)
                    {
                        equippedItemDict[slot] = null;
                    }
                    else
                    {
                        equippedItemDict[slot] = await ConvertToItemAsync(kvp.Value as Dictionary<string, object>);
                    }
                }
                Debug.Log("저장된 데이터 불러오기 성공!");
                OnInventoryLoaded?.Invoke(equippedItemDict);
            }
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Inventory 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public async void SaveInventory(Dictionary<EEquipmentSlot, Item> equippedItemDict)
    {
        DocumentReference docRef = FirebaseManager.Instance.DB.Collection("Inventory").Document(USER_ID);

        Dictionary<string, SerializableItem> saveData = ConvertInventoryData(equippedItemDict);
        try
        {
            await docRef.SetAsync(new Dictionary<string, object> { { "Equipments", saveData } });
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemStorage 데이터 저장 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }
    }

    public Dictionary<string, List<SerializableItem>> ConvertItemStorageData(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
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

    public Dictionary<string, SerializableItem> ConvertInventoryData(Dictionary<EEquipmentSlot, Item> equippedItemDict)
    {
        var saveData = new Dictionary<string, SerializableItem>();
        foreach (var kvp in equippedItemDict)
        {
            string slotKey = kvp.Key.ToString();
            if (kvp.Value == null)
            {
                saveData[slotKey] = null;
            }
            else
            {
                saveData[slotKey] = new SerializableItem(kvp.Value);
            }
        }

        return saveData;
    }


    private async Task<Item> ConvertToItemAsync(Dictionary<string, object> dict)
    {
        if (string.IsNullOrEmpty((string)dict["ImageAddress"]))
        {
            throw new Exception("어드레서블 주소가 없습니다.");
        }

        var sprite = await Addressables.LoadAssetAsync<Sprite>(dict["ImageAddress"]).Task;

        Item loadedItem = new Item(
            id: dict["ID"] as string,
            name: dict["Name"] as string,
            description: dict["Description"] as string,
            image: sprite,
            imageAddress: dict["ImageAddress"] as string,
            equipmentSlot: Enum.TryParse(dict["EquipmentSlot"] as string, out EEquipmentSlot slot) ? slot : default,
            isEquipped: dict.ContainsKey("IsEquipped") && (bool)dict["IsEquipped"]
        );

        return loadedItem;
    }
}

[FirestoreData]
public class SerializableItem
{
    [FirestoreProperty] public string ID { get; set; }
    [FirestoreProperty] public string Name { get; set; }
    [FirestoreProperty] public string Description { get; set; }
    [FirestoreProperty] public string ImageAddress { get; set; }
    [FirestoreProperty] public string EquipmentSlot { get; set; }
    [FirestoreProperty] public bool IsEquipped { get; set; }

    public SerializableItem()
    {

    }

    public SerializableItem(Item item)
    {
        if (string.IsNullOrEmpty(item.ID))
            {
                throw new Exception("ID가 비어있습니다.");
            }

        if (string.IsNullOrEmpty(item.Name))
        {
            throw new Exception("아이템 이름이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(item.Description))
        {
            throw new Exception("아이템 설명이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(item.ImageAddress))
        {
            throw new Exception("아이콘 이미지 주소가 없습니다.");
        }

        ID = item.ID;
        Name = item.Name;
        Description = item.Description;
        ImageAddress = item.ImageAddress;
        EquipmentSlot = item.EquipmentSlot.ToString();
        IsEquipped = item.IsEquipped;
    }
}
