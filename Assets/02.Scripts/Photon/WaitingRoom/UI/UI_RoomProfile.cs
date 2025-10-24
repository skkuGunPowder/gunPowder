using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_RoomProfile : MonoBehaviour
{
    public List<UI_ProfileSlot> UI_ProfileSlotList = new List<UI_ProfileSlot>();
    
    private void OnEnable()
    {
        EventManager.Instance.OnRoomDataChanged += Refresh;
        EventManager.Instance.OnReadyChanged += ReadyCheck;
        EventManager.Instance.OnTeamChanged += TeamChange;
    }
    public void Refresh()
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerList.PlayerSlotList;
        
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                UI_ProfileSlotList[i].Refresh(null);
                continue;
            }
            
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);
            
            UI_ProfileSlotList[i].Refresh(player);
            
            if (player.IsMasterClient)
            {
                UI_ProfileSlotList[i].MasterCheck(true);   
            }
            else
            {
                UI_ProfileSlotList[i].ReadyCheck(player); 
            }
        }
    }
    
    public void ReadyCheck(PhotonPlayer player)
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerList.PlayerSlotList;

        for (int i = 0; i < playerSlotList.Count; i++)
        {
            // 건너뛰기
            if (playerSlotList[i] == 0)
            {
                continue;
            }

            if (playerSlotList[i] != player.ActorNumber)
            {
                continue;
            }
            
            if (player.IsMasterClient)
            {
               UI_ProfileSlotList[i].MasterCheck(true);   
            }
            else
            {
                UI_ProfileSlotList[i].ReadyCheck(player);
            }
        }
    }

    public void TeamChange()
    {
        List<int> playerSlotList = RoomManager.Instance.PlayerList.PlayerSlotList;
        
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                continue;
            }
            
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);
            UI_ProfileSlotList[i].TeamSet(player);
        }
    }
    
    private void OnDisable()
    {
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
