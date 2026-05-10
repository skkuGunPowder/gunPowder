using UnityEngine;
using BackEnd;
using LitJson;
using System.Collections.Generic;
using System;

public class BuffRepository
{
    public event Action<Dictionary<string, BuffStat>> OnStatLoaded;

    public BuffRepository()
    {
        LoadBuffData();
    }

    public void LoadBuffData()
    {
        Dictionary<string, BuffStat> statDict = new Dictionary<string, BuffStat>();

        Backend.Chart.GetChartListByFolderV2(BackendManager.FOLDER_ID, result =>
        {
            if (!result.IsSuccess())
            {
                Debug.LogError($"[BUFF] DEV 차트 불러오기 실패: {result.GetMessage()}");
                return;
            }
            
            foreach (JsonData chart in result.FlattenRows())
            {
                if (chart["chartName"].ToString() != "Buff")
                {
                    continue;
                }

                var chartContents = Backend.Chart.GetChartContents(chart["selectedChartFileId"].ToString());
                if (!chartContents.IsSuccess())
                {
                    Debug.LogError($"버프 데이터 불러오기 실패: {chartContents.GetMessage()}");
                    continue;
                }

                foreach (JsonData buffInfo in chartContents.FlattenRows())
                {
                    BuffStat buffStat = new BuffStat(buffInfo);
                    statDict.Add((string)buffInfo["BuffID"], buffStat);
                }
            }
            OnStatLoaded?.Invoke(statDict);
        });
    }
}
