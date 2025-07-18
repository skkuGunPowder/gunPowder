using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemDatabaseRepo
{
    public event Action<Dictionary<string, Item>> OnLoadItemData;


    public async Task<Dictionary<string, Item>> LoadItemData()
    {
        Dictionary<string, Item> itemData = new Dictionary<string, Item>();

        CollectionReference itemDatabaseRef = FirebaseManager.Instance.DB.Collection("Items");
        try
        {
            QuerySnapshot snapshots = await itemDatabaseRef.GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshots.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> itemRawData = document.ToDictionary();
                    Item item = await ConvertToItemAsync(document.Id, itemRawData);
                    itemData[document.Id] = item;
                }
            }
            Debug.Log("ItemData 불러오기 성공!");
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"ItemData 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }

        return itemData;
    }

    public async Task<Dictionary<string, IStat>> LoadStatData()
    {
        Dictionary<string, IStat> statData = new Dictionary<string, IStat>();

        CollectionReference itemDatabaseRef = FirebaseManager.Instance.DB.Collection("Stats");
        try
        {

            QuerySnapshot snapshots = await itemDatabaseRef.GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshots.Documents)
            {
                if (document.Exists)
                {
                    BombStat stat = document.ConvertTo<BombStat>();
                    statData[document.Id] = stat;
                }
            }
            Debug.Log("StatData 불러오기 성공!");
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"StatData 데이터 로드 실패. 에러코드 {e.ErrorCode} : {e.Message}");
        }

        return statData;
    }

    private async Task<Item> ConvertToItemAsync(string id, Dictionary<string, object> dict)
    {
        // 저장된 데이터 -> Item 객체로 변환하는 메소드

        string imageAddress = (string)dict["ImageAddress"];

        // 유효성 검사
        if (string.IsNullOrEmpty(imageAddress))
        {
            throw new Exception("어드레서블 주소가 없습니다.");
        }

        // 어드레서블 이미지 로드
        Sprite itemImage = await Addressables.LoadAssetAsync<Sprite>(imageAddress).Task;

        // Item객체로 반환
        return new Item(
            id: id,
            itemType: Enum.TryParse((string)dict["ItemType"], out EItemType slot) ? slot : EItemType.None,
            name: (string)dict["Name"],
            explanation: (string)dict["Explanation"],
            imageAddress: imageAddress,
            image : itemImage
        );
    }
}