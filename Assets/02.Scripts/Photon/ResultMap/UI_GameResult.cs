using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameResult : MonoBehaviour
{
    public List<UI_GameResultSlot> UI_GameResultSlotList = new List<UI_GameResultSlot>();
    private void Awake()
    {
        EventManager.Instance.OnGameResult += Refresh;
    }

    private void Refresh()
    {
        List<GameResultData> dataList = GameResultManager.Instance.ResultDataList;

        for (int i = 0; i < UI_GameResultSlotList.Count; i++)
        {
            GameResultData data = dataList[i];
            if (i < dataList.Count)
            {
                UI_GameResultSlotList[i].gameObject.SetActive(true);
                UI_GameResultSlotList[i].Refresh(data.Player.ActorNumber,data.Damage, data.SurviveTime, data.Kill);
            }
            else
            {
                UI_GameResultSlotList[i].gameObject.SetActive(false);
            }
         
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnGameResult -= Refresh;    }
}
