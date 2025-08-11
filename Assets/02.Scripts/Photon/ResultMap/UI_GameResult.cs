using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameResult : MonoBehaviour
{
    public List<UI_GameResultSlot> UI_GameResultSlotList = new List<UI_GameResultSlot>();
    public GameObject Header;
    public  List<GameResultData> dataList = new List<GameResultData>();
    public float Timer = 0.4f;

    private void Awake()
    {
        EventManager.Instance.OnGameResult += Refresh;
    }
    
    private void Refresh()
    {
        Header.SetActive(true);

        StartCoroutine(Coroutine_Refresh());
    }

    private IEnumerator Coroutine_Refresh()
    {
        // List<GameResultData> dataList = GameResultManager.Instance.ResultDataList;
        
        for (int i = 0; i < UI_GameResultSlotList.Count; i++)
        {
            
            if (i < dataList.Count)
            {
                GameResultData data = dataList[i];
                UI_GameResultSlotList[i].gameObject.SetActive(true);
                // UI_GameResultSlotList[i].Refresh(data.Player,data.Damage, data.Rank, data.SurviveTime, data.Kill,data.Team);
            }
            else
            {
                UI_GameResultSlotList[i].gameObject.SetActive(false);
            }
            
            yield return new WaitForSeconds(Timer);
         
        }
        
        yield return new WaitForSeconds(Timer);
        GameResultManager.Instance.LoadScene();
    }

    private void SlotOff()
    {
        foreach (UI_GameResultSlot slot  in  UI_GameResultSlotList)
        {
            slot.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        SlotOff();
        Header.SetActive(false);
        StopAllCoroutines();
        EventManager.Instance.OnGameResult -= Refresh;
        
    }
}
