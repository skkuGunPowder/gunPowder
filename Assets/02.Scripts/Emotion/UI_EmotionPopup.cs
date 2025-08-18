using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class UI_EmotionPopup : UI_Popup
{
    public EEmotion CurrentEmotion;
    
    private List<int> _myEmotionList;

    public List<EmotionSlot> _emotionSlotList;
    private void OnEnable()
    {
        _myEmotionList = new List<int>();
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Emotion.ToString()) == false)
        {
            for(int i = 0; i < 10; i++)
            {
                _myEmotionList.Add(i);
            }
        }
        else
        {
            int[] emotions = (int[])PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Emotion.ToString()];
            _myEmotionList = new List<int>(emotions);
   
        }
        for (int i = 0; i < _myEmotionList.Count; i++)
        {
            _emotionSlotList[i].Emotion = (EEmotion)_myEmotionList[i];
            _emotionSlotList[i].Refresh();
        }
    }

    public void OnClickAccept()
    {
        _myEmotionList.Clear();
        
        for (int i = 0; i < _emotionSlotList.Count; i++)
        {
            _myEmotionList.Add((int)_emotionSlotList[i].Emotion);
        }
        Hashtable emotions = new Hashtable()
        {
            {EProperties.Emotion.ToString(), _myEmotionList.ToArray()}
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(emotions);
    }
    
}
