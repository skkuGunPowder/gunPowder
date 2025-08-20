using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_RoomProfile : MonoBehaviour
{
    public List<UI_ProfileSlot> UI_ProfileSlotList = new List<UI_ProfileSlot>();
    
    private void Awake()
    {
        Debug.Log("roomprofile awake");
        // EventManager.Instance.OnPlayEmotion += PlayEmotion;
        EventManager.Instance.OnRoomDataChanged += Refresh;
        EventManager.Instance.OnReadyChanged += ReadyCheck;
        EventManager.Instance.OnTeamChanged += TeamChange;
        
    }
    
    public void Refresh()
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerSlotList;
        
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                UI_ProfileSlotList[i].Refresh(null);
                continue;
            }
            
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);
            UI_ProfileSlotList[i].Refresh(player);
            
        }
    }
    
    public void ReadyCheck()
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerSlotList;

        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                continue;
            }
            
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);

            if (player == null)
            {
                return;
            }
            
            if (player.IsMasterClient)
            {
               UI_ProfileSlotList[i].MasterCheck(true);   
            }
            else
            {
                UI_ProfileSlotList[i].ReadyCheck((bool)player.CustomProperties[$"{EProperties.IsReady}"]);
            }
        }
    }

    public void TeamChange()
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerSlotList;
        
        Debug.Log($"{playerSlotList.Count}");
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                continue;
            }
            
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);
            
            if (player.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
            {
                EInGameTeam team = EInGameTeam.Red;
                UI_ProfileSlotList[i].TeamSet(team);
            }
            else
            {
                EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
                UI_ProfileSlotList[i].TeamSet(team);
            }
        }
    }

    // private void PlayEmotion( string emotion,int playerNumber)
    // {
    //     List<int> playerList = RoomManager.Instance.PlayerSlotList;
    //     for (int i = 0; i < playerList.Count; i++)
    //     {
    //         
    //         if (playerList[i] == 0)
    //         {
    //             continue;
    //         }
    //
    //         if (playerList[i] == playerNumber)
    //         {
    //             UI_ProfileSlotList[i].Play(emotion);
    //             break;
    //         }
    //     }
    // }
    private void OnDisable()
    {
        // EventManager.Instance.OnPlayEmotion -= PlayEmotion;
        EventManager.Instance.OnRoomDataChanged -= Refresh;
        EventManager.Instance.OnReadyChanged -= ReadyCheck;
        EventManager.Instance.OnTeamChanged -= TeamChange;
    }

    //  ⊂_ヽ
    //　  ＼＼  Λ＿Λ
    //　　 ＼( ‘ㅅ' ) 두둠칫
    //　　　 >　⌒ヽ
    //　　　/ 　 へ＼
    //　　 /　　/　＼＼
    //　　 ﾚ　ノ　　 ヽ_つ
    //　　/　/두둠칫
    //　 /　/|
    //　(　(ヽ
    //　|　|、＼
    //　| 丿 ＼ ⌒)
    //　| |　　) /
    //`ノ )　　Lﾉ 
}
