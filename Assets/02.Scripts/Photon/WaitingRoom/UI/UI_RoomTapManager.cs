using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoomTapManager : MonoBehaviour
{
    [SerializeField] private List<TapData> _tapDataList = new List<TapData>();
    
    private Dictionary<ETapOption, GameObject> _tapObjectDictionary = new ();
    private Dictionary<ETapOption, Button> _tapButtonDictionary = new ();

    private ETapOption _currentTap;


    private void Awake()
    {
        foreach (TapData data in _tapDataList)
        {
            _tapObjectDictionary[data.TapType] = data.TapObject;
            _tapButtonDictionary[data.TapType] = data.TapButton;
            
            ETapOption tap = data.TapType;
            
            if (data.IsMaster)
            {
                data.TapButton.onClick.AddListener((() => OnclickOnlyMaster(tap)));
                continue;
            }
            
            data.TapButton.onClick.AddListener((() => Onclick(tap)));
        }
    }

    public void Onclick(ETapOption tapType)
    {
        SetActiveTap(tapType);
    }

    public void OnclickOnlyMaster(ETapOption tapType)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        SetActiveTap(tapType);
    }

    private void SetActiveTap(ETapOption tapType)
    {
        foreach (var tap  in _tapObjectDictionary)
        {
            tap.Value.SetActive(tap.Key == tapType);
        }

        foreach (var button in _tapButtonDictionary)
        {
            bool isSelected = button.Key == tapType;
            {
                button.Value.interactable = isSelected == false;
            }
        }

        _currentTap = tapType;
    }
}
