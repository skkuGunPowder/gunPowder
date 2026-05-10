using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd;
using LitJson;
using UnityEngine;

public class TextRepository
{
    //뒤끝 차트로부터 받아온 텍스트 데이터들을 Dictionary 형태로 저장하는 클래스
    private Dictionary<string, TextData> textData = new Dictionary<string, TextData>();

    public Task<Dictionary<string, TextData>> LoadedTextData()
    {
        TaskCompletionSource<Dictionary<string, TextData>> loadCompletionSource = new TaskCompletionSource<Dictionary<string, TextData>>();
        Dictionary<string, TextData> dataDict = new Dictionary<string, TextData>();

        Backend.Chart.GetChartListByFolderV2(BackendManager.FOLDER_ID, result =>
        {
            if (!result.IsSuccess())
            {
                Debug.LogError($"DEV 폴더 불러오기 실패: {result.GetMessage()}");
                loadCompletionSource.TrySetResult(dataDict);
                return;
            }

            foreach (JsonData chart in result.FlattenRows())
            {
                if (chart["chartName"].ToString() != "Translate")
                {
                    continue;
                }

                var chartContents = Backend.Chart.GetChartContents(chart["selectedChartFileId"].ToString());
                if (!chartContents.IsSuccess())
                {
                    Debug.LogError($"텍스트 데이터 불러오기 실패: {chartContents.GetMessage()}");
                    continue;
                }

                foreach (JsonData textInfo in chartContents.FlattenRows())
                {
                    TextData textData = new TextData(textInfo);
                    dataDict[textData.ID] = textData;
                }
            }

            loadCompletionSource.TrySetResult(dataDict);
        });

        return loadCompletionSource.Task;
    }
}
