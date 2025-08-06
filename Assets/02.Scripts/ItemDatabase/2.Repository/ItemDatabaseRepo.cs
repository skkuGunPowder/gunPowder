using System;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class ItemDatabaseRepo
{
    private const int ITEM_DATA_FOLDER_ID = 2594;

    public event Action<Dictionary<string, Item>, Dictionary<string, IStat>> OnitemDataLoaded;


    public void Init()
    {
        LoadItemDataAsync();
    }

    public void LoadItemDataAsync()
    {
        Dictionary<string, Item> itemDataDict = new Dictionary<string, Item>();
        Dictionary<string, IStat> statDataDict = new Dictionary<string, IStat>();


        Backend.Chart.GetChartListByFolderV2(ITEM_DATA_FOLDER_ID, result =>
        {
            if (!result.IsSuccess())
            {
                Debug.LogError($"아이템 데이터 불러오기 실패: {result.GetMessage()}");
                return;
            }

            JsonData chartListData = result.GetReturnValuetoJSON();
            foreach (JsonData chart in chartListData["rows"])
            {
                var itemResult = Backend.Chart.GetChartContents(chart["selectedChartFileId"]["N"].ToString());
                if (!itemResult.IsSuccess())
                {
                    Debug.LogError($"아이템 데이터 불러오기 실패: {itemResult.GetMessage()}");
                    continue;
                }

                foreach (JsonData iteminfo in itemResult.GetReturnValuetoJSON()["rows"])
                {
                    Item itemObj = ConvertToItem(iteminfo);
                    itemDataDict[itemObj.ID] = itemObj;

                    if (itemObj.ID[0] == 'B')
                    {
                        BombStat bombStat = new BombStat(iteminfo);
                        statDataDict[itemObj.ID] = bombStat;
                        continue;
                    }

                    if (itemObj.ID[0] == 'E')
                    {
                        ExplosionStat bombStat = new ExplosionStat(iteminfo);
                        statDataDict[itemObj.ID] = bombStat;
                        continue;
                    }
                }
            }
            Debug.Log("아이템 데이터 불러오기 성공");

            OnitemDataLoaded?.Invoke(itemDataDict, statDataDict);
        });
    }

    private Item ConvertToItem(JsonData json)
    {
        string id = json["MYID"].ToString();
        EItemType itemType = (EItemType)Enum.Parse(typeof(EItemType), json["ItemType"].ToString());
        string name = json["Name"].ToString();
        string explanation = json["Explanation"].ToString();
        string imageAddress = json["ImageAddress"].ToString();
        string prefabAddress = json["PrefabAddress"].ToString();

        Sprite itemImage = Addressables.LoadAssetAsync<Sprite>(imageAddress).WaitForCompletion();
        GameObject itemPrefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();

        return new Item(id, itemType, name, explanation, imageAddress, prefabAddress, itemImage, itemPrefab);
    }
}