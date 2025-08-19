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
    
    private List<PlayerEmotion> _playerEmotionList = new List<PlayerEmotion>();
    private void Awake()
    {
        _myEmotionList = new List<int>();
        EventManager.Instance.OnPlayerFind += Request_PlayerFind;
    }

    private void Request_PlayerFind()
    {
        MyPhotonView.RPC(nameof(RPC_PlayerFind), RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_PlayerFind()
    {
        PlayerEmotion[] playerEmotionArray = FindObjectsByType<PlayerEmotion>(FindObjectsSortMode.None);
        _playerEmotionList = new List<PlayerEmotion>(playerEmotionArray);
        
        Debug.Log($"Find Player {_playerEmotionList.Count} Find : {_playerEmotionList[0].GetPlayerNumber()}");
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
        MyPhotonView.RPC(nameof(RPC_PlayEmotion), RpcTarget.All, emotionId);
        
    }
    
    [PunRPC]
    private void RPC_PlayEmotion(int emotionId, PhotonMessageInfo info)
    {
        Debug.Log($"Play Emotion {info.Sender.NickName} {emotionId}");
        
        string emotionName = Enum.GetName(typeof(EEmotion), emotionId);
        PhotonPlayer player = info.Sender;

        foreach (var emotion in _playerEmotionList)
        {
            if (emotion.GetPlayerNumber() == player.ActorNumber)
            {
                if (emotion.IsLive == false)
                {
                    Debug.Log($"Dead Emotion {emotionName} {player.NickName}");
                    EventManager.Instance.PlayEmotion(emotionName, player.ActorNumber);
                    break;   
                }
                
                emotion.Play(emotionName);
                
                break;
            }
        }
    }
    
    private void DefaultEmotion()
    {
        for (int i = 0; i < 10; i++)
        {
            _myEmotionList.Add(i);
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnPlayerFind -= Request_PlayerFind;
    }
}
