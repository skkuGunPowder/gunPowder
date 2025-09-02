using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UI_RoomTapManager : MonoBehaviour
{
    public List<UI_TapSlot> TapDataList = new List<UI_TapSlot>();
    
    private void Awake()
    {
        Init();
        
        SelectTap(ETapOption.Tap_Start);
    }

    private void Init()
    {
        foreach (UI_TapSlot data in TapDataList)
        {
            ETapOption tap = data.TapType;
            
            if (data.IsMaster)
            {
                data.TapButton.onClick.AddListener(() => OnclickOnlyMaster(tap));
                continue;
            }
            
            data.TapButton.onClick.AddListener(() => Onclick(tap));
        }
    }
    public void SelectTap(ETapOption tapType)
    {
        foreach (var tap in TapDataList)
        {
            tap.SetActive(tapType);
        }
    }
    public void Onclick(ETapOption tapType)
    {
        SelectTap(tapType);
    }

    public void OnclickOnlyMaster(ETapOption tapType)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        } 
        
        SelectTap(tapType);
    }
}
