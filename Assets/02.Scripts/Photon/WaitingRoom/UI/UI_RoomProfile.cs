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
        EventManager.Instance.OnRoomDataChanged += Refresh;
        EventManager.Instance.OnReadyChanged += ReadyCheck;
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

    private void OnDisable()
    {
        EventManager.Instance.OnRoomDataChanged -= Refresh;
        EventManager.Instance.OnReadyChanged -= ReadyCheck;
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
