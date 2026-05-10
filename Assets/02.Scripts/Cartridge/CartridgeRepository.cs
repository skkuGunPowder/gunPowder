using UnityEngine;
using BackEnd;
using LitJson;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CartridgeRepository
{
    public Task<Dictionary<string, CartridgeData>> LoadCartridgeDataAsync()
    {
        TaskCompletionSource<Dictionary<string, CartridgeData>> loadCompletionSource = new TaskCompletionSource<Dictionary<string, CartridgeData>>();
        Dictionary<string, CartridgeData> dataDict = new Dictionary<string, CartridgeData>();

        Backend.Chart.GetChartListByFolderV2(BackendManager.FOLDER_ID, result =>
        {
            if (!result.IsSuccess())
            {
                Debug.LogError($"카트리지 데이터 불러오기 실패: {result.GetMessage()}");
                loadCompletionSource.TrySetResult(dataDict);
                return;
            }

            foreach (JsonData chart in result.FlattenRows())
            {
                if (chart["chartName"].ToString() != "Cartridge")
                {
                    continue;
                }

                var chartContents = Backend.Chart.GetChartContents(chart["selectedChartFileId"].ToString());
                if (!chartContents.IsSuccess())
                {
                    Debug.LogError($"카트리지 데이터 불러오기 실패: {chartContents.GetMessage()}");
                    continue;
                }

                foreach (JsonData cartridgeInfo in chartContents.FlattenRows())
                {
                    CartridgeData cartridgeData = new CartridgeData(cartridgeInfo);
                    dataDict[cartridgeData.ID] = cartridgeData;
                }
            }

            loadCompletionSource.TrySetResult(dataDict);
        });

        return loadCompletionSource.Task;
    }
}
