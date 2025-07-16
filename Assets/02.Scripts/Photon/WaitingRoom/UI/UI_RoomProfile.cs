using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_RoomProfile : MonoBehaviour
{
    public List<UI_ProfileSlot> UI_ProfileSlotList = new List<UI_ProfileSlot>();

    private void Start()
    {
        RoomManager.Instance.OnDataChanged += Refresh;
    }

    public void Refresh()
    {
        Debug.Log("RoomProfile slot = Refresh");

        List<int> playerSlotList = RoomManager.Instance.PlayerSlotList;
        
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            Debug.Log($"1번 포문 : {playerSlotList[i]}");
        }
        
        for (int i = 0; i < playerSlotList.Count; i++)
        {
            if (playerSlotList[i] == 0)
            {
                Debug.Log($"2번 포문 : {playerSlotList[i]}");
                continue;
            }
            
            Debug.Log($"{i}");
           
            PhotonPlayer player = PhotonNetwork.CurrentRoom.GetPlayer(playerSlotList[i]);
            UI_ProfileSlotList[i].Refresh(player);
            
        }
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
