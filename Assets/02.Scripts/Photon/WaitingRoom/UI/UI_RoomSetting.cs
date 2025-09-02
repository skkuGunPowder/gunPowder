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
    public UI_RoomSetupButton Decline;
    public UI_RoomSetupButton Life;
    
    // 처음 설정한 방 세팅 가져오기
    private void OnEnable()
    {
        PlaytimeInit = Init(ERoomProperties.PlayTime,Playtime);
        PowderInit = Init(ERoomProperties.Gunpowder,Powder);
        DeclineInit = Init(ERoomProperties.DeclinePowder,Decline);
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

    public void AcceptButton()
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        
        Hashtable roomProperties = new Hashtable
        {
            {$"{ERoomProperties.PlayTime}", Playtime.CurrentValue()},
            {$"{ERoomProperties.Life}",  Life.CurrentValue()},
            {$"{ERoomProperties.Gunpowder}",  Powder.CurrentValue()},
            {$"{ERoomProperties.DeclinePowder}",  Decline.CurrentValue()},
        };
        
        currentRoom.SetCustomProperties(roomProperties);

        PlaytimeInit = Playtime.CurrentValue();
        PowderInit = Powder.CurrentValue();
        DeclineInit = Decline.CurrentValue();
        LifeInit = Life.CurrentValue();
    }

    public void CancelButton()
    {
       Playtime.Reset(PlaytimeInit);
       Powder.Reset(PowderInit);
       Decline.Reset(DeclineInit);
       Life.Reset(LifeInit);
    }
}
