using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class EmotionManager : MonoBehaviour
{ 
    public PhotonView MyPhotonView;
    private List<int> _myEmotionList;

    private Dictionary<KeyCode, int> _emotionKeyDictionary;
    private void Awake()
    {
        _myEmotionList = new List<int>();   
        MyPhotonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Emotion.ToString()) == false)
        {
            DefaultEmotion();
        }
        else
        {
            int[] emotions = (int[])PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Emotion.ToString()];
            _myEmotionList = new List<int>(emotions);
        }
        
        _emotionKeyDictionary = new Dictionary<KeyCode, int>();
       
        for (int i = 0; i < _myEmotionList.Count; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;  
            _emotionKeyDictionary.Add(key, _myEmotionList[i]); 
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
        if (MyPhotonView.IsMine == false)
        {
            return;
        }
        
        MyPhotonView.RPC(nameof(RPC_PlayEmotion), RpcTarget.All, emotionId);
    }
    
    [PunRPC]
    private void RPC_PlayEmotion(int emotionId, PhotonMessageInfo info)
    {
        string emotionName = Enum.GetName(typeof(EEmotion), emotionId);
        PhotonPlayer player = info.Sender;
        
        EventManager.Instance.PlayEmotion(emotionName, player.ActorNumber);
    }
    
    private void DefaultEmotion()
    {
        for (int i = 0; i < 10; i++)
        {
            _myEmotionList.Add(i);
        }
    }
}
