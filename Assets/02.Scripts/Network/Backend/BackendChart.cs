using UnityEngine;
using BackEnd;

public class BackendChart : Singleton<BackendChart>
{
    private const string BombChartID = "197295";

    public void GetChart(string chartID)
    {
        Backend.Chart.GetChartContents(chartID, bro =>
        {
            if (bro.IsSuccess() == false)
            {
                Debug.LogError($"[BackendChart] 차트 조회 실패 : {bro.ErrorCode} | {bro.Message}");
                return;
            }
        });
    }
}
