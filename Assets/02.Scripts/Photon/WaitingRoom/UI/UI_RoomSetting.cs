using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class UI_RoomSetting : MonoBehaviour
{
    private int PlaytimeInit;
    private int PowderInit;
    private int DeclineInit;
    private int LifeInit;
    
    public UI_RoomSetupButton Playtime;
    public UI_RoomSetupButton Powder;
    public UI_RoomSetupButton Life;
    
    // 처음 설정한 방 세팅 가져오기
    private void OnEnable()
    {
        PlaytimeInit = Init(ERoomProperties.PlayTime,Playtime);
        PowderInit = Init(ERoomProperties.Gunpowder,Powder);
        LifeInit = Init(ERoomProperties.Life, Life);
    }
    
    private int Init(ERoomProperties properties, UI_RoomSetupButton button)
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        
        int value = int.Parse(currentRoom.CustomProperties[properties.ToString()].ToString());
        
        button.InitValue = value;
        button.Init();
        
        return value;
    }

    public void OnclickDeathmatch()
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        Hashtable roomProperties = new Hashtable
        {
            {ERoomProperties.GameMode.ToString(), (int)EGameMode.Deathmatch}
        };
        currentRoom.SetCustomProperties(roomProperties);
    }
    
    public void OnclickInfinite()
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        Hashtable roomProperties = new Hashtable
        {
            {ERoomProperties.GameMode.ToString(), (int)EGameMode.Infinite}
        };
        currentRoom.SetCustomProperties(roomProperties);
    }
    public void AcceptButton()
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        
        Hashtable roomProperties = new Hashtable
        {
            {$"{ERoomProperties.PlayTime}", Playtime.CurrentValue()},
            {$"{ERoomProperties.Life}",  Life.CurrentValue()},
            {$"{ERoomProperties.Gunpowder}",  Powder.CurrentValue()}
        };
        
        currentRoom.SetCustomProperties(roomProperties);

        PlaytimeInit = Playtime.CurrentValue();
        PowderInit = Powder.CurrentValue();
        LifeInit = Life.CurrentValue();
    }

    public void CancelButton()
    {
       Playtime.Reset(PlaytimeInit);
       Powder.Reset(PowderInit);
       Life.Reset(LifeInit);
    }
}
