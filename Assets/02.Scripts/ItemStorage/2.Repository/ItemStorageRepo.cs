using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemStorageRepo
{
    private const string USER_ID = "USER_01";

    public event Action<Dictionary<EEquipmentSlot, List<Item>>> OnItemStorageLoaded;
    public event Action<Dictionary<EEquipmentSlot, Item>> OnInventoryLoaded;


    public async void SaveItemStorage(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        // FireStore에 저장가능한 형태로 데이터 변환
        Dictionary<string, List<SerializableItem>> saveData = ConvertItemStorageData(itemDict);

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
                var itemDict = new Dictionary<EEquipmentSlot, List<Item>>();
                
                // 아이템 딕셔너리에 데이터 할당
                foreach (var kvp in rawItemStorageData)
                {
                    string key = kvp.Key;
                    if (!Enum.TryParse(key, out EEquipmentSlot slot))
                    {
                        continue;
                    }

                    var itemList = new List<Item>();
                    // 아이템 리스트에 데이터 할당
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
        // 데이터 로드
        CollectionReference inventoryRef = FirebaseManager.Instance.DB.Collection("Inventory");
        try
        {
            DocumentSnapshot document = await inventoryRef.Document(USER_ID).GetSnapshotAsync();
            if (document.Exists)
            {
                var rawInventoryData = document.GetValue<Dictionary<string, object>>("Equipments");
                var equippedItemDict = new Dictionary<EEquipmentSlot, Item>();

                // 장착된 아이템 딕셔너리에 데이터 할당
                foreach (var kvp in rawInventoryData)
                {
                    string key = kvp.Key;

                    // 아이템 데이터 없을 시 null 할당
                    if (Enum.TryParse(key, out EEquipmentSlot slot))
                    {
                        equippedItemDict[slot] = null;
                        continue;
                    }

                    // 아이템 데이터 있을 시 Item 객체로 변환하여 할당
                    equippedItemDict[slot] = await ConvertToItemAsync(kvp.Value as Dictionary<string, object>);
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

    public Dictionary<string, List<SerializableItem>> ConvertItemStorageData(Dictionary<EEquipmentSlot, List<Item>> itemDict)
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

    public Dictionary<string, SerializableItem> ConvertInventoryData(Dictionary<EEquipmentSlot, Item> equippedItemDict)
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


    private async Task<Item> ConvertToItemAsync(Dictionary<string, object> dict)
    {
        // 저장된 데이터 -> Item 객체로 변환하는 메소드

        // 유효성 검사
        if (string.IsNullOrEmpty((string)dict["ImageAddress"]))
        {
            throw new Exception("어드레서블 주소가 없습니다.");
        }

        // 스프라이트 에셋 로드
        var sprite = await Addressables.LoadAssetAsync<Sprite>(dict["ImageAddress"]).Task;

        // Item객체로 변환
        Item loadedItem = new Item(
            id: dict["ID"] as string,
            name: dict["Name"] as string,
            explanation: dict["Description"] as string,
            image: sprite,
            imageAddress: dict["ImageAddress"] as string,
            itme: Enum.TryParse(dict["EquipmentSlot"] as string, out EEquipmentSlot slot) ? slot : default,
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

        if (string.IsNullOrEmpty(item.Explanation))
        {
            throw new Exception("아이템 설명이 비어있습니다.");
        }

        if (string.IsNullOrEmpty(item.ImageAddress))
        {
            throw new Exception("아이콘 이미지 주소가 없습니다.");
        }

        ID = item.ID;
        Name = item.Name;
        Explanation = item.Explanation;
        ImageAddress = item.ImageAddress;
        EquipmentSlot = item.ItemType.ToString();
        IsEquipped = item.IsEquipped;
    }
}
