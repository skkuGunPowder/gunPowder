using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class EmotionManager : MonoBehaviour
{ 
    private PhotonView _photonView;
    public List<int> MyEmotionList;

    private Dictionary<KeyCode, int> _emotionKeyDictionary;
    private void Awake()
    {
        MyEmotionList = new List<int>();   
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        int[] emotions = (int[])PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Emotion.ToString()];
        MyEmotionList = new List<int>(emotions);
        
        _emotionKeyDictionary = new Dictionary<KeyCode, int>();
        for (int i = 0; i < MyEmotionList.Count; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;  
            _emotionKeyDictionary.Add(key, MyEmotionList[i]); 
        }
    }

    private void Update()
    {
        foreach (var kvp in _emotionKeyDictionary)
        {
            if (InputHandler.GetKeyDown(kvp.Key))
            {
                Request_PlayEmotion(kvp.Value);
            }
        }
    }

    private void Request_PlayEmotion(int emotionId)
    {
        if (_photonView.IsMine == false)
        {
            return;
        }
        
        _photonView.RPC(nameof(RPC_PlayEmotion), RpcTarget.All, emotionId);
    }
    
    [PunRPC]
    private void RPC_PlayEmotion(int emotionId, PhotonMessageInfo info)
    {
        string emotionName = Enum.GetName(typeof(EEmotion), emotionId);
        PhotonPlayer player = info.Sender;
        
        EventManager.Instance.PlayEmotion(emotionName, player.ActorNumber);
    }
}
