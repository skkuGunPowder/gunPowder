using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd;
using LitJson;
using UnityEngine;


public class ItemDatabaseRepo
{
    private const int ITEM_DATA_FOLDER_ID = 2594;

    public event Action<Dictionary<string, Item>, Dictionary<string, IStat>> OnitemDataLoaded;

    private bool _isItemLoadDone = false;
    private bool _isExplosionLoadDone = false;


    public void Init()
    {
        LoadItemDataAsync();
    }

    private void CheckComplete(Dictionary<string, Item> itemDataDict, Dictionary<string, IStat> statDataDict)
    {
        if (_isExplosionLoadDone && _isItemLoadDone)
        {
            OnitemDataLoaded?.Invoke(itemDataDict, statDataDict);
        }
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

            foreach (JsonData chart in result.FlattenRows())
            {
                var itemResult = Backend.Chart.GetChartContents(chart["selectedChartFileId"].ToString());
                if (!itemResult.IsSuccess())
                {
                    Debug.LogError($"아이템 데이터 불러오기 실패: {itemResult.GetMessage()}");
                    continue;
                }

                foreach (JsonData iteminfo in itemResult.FlattenRows())
                {
                    Item item = new Item(iteminfo);
                    itemDataDict.Add(item.ID, item);

                    if (item.ID[0] == 'B')
                    {
                        BombStat bombStat = new BombStat(iteminfo);
                        statDataDict.Add(item.ID, bombStat);
                    }
                }
            }
            Debug.Log("아이템 데이터 불러오기 성공");
            _isItemLoadDone = true;
            CheckComplete(itemDataDict, statDataDict);
        });

        Backend.Chart.GetChartListV2(result =>
        {
            if (!result.IsSuccess())
            {
                Debug.LogError($"폭발 데이터 불러오기 실패: {result.GetMessage()}");
                return;
            }

            foreach (JsonData chart in result.FlattenRows())
            {
                if (chart["chartName"].ToString() != "Explosion")
                {
                    continue;
                }

                var chartContents = Backend.Chart.GetChartContents(chart["selectedChartFileId"].ToString());
                if (!chartContents.IsSuccess())
                {
                    Debug.LogError($"폭발 데이터 불러오기 실패: {chartContents.GetMessage()}");
                    continue;
                }

                foreach (JsonData explosioninfo in chartContents.FlattenRows())
                {
                    ExplosionStat explosionStat = new ExplosionStat(explosioninfo);
                    statDataDict.Add((string)explosioninfo["ItemID"], explosionStat);
                }
            }
            Debug.Log("폭발 데이터 불러오기 성공");
            _isExplosionLoadDone = true;
            CheckComplete(itemDataDict, statDataDict);
        });  
    }
}