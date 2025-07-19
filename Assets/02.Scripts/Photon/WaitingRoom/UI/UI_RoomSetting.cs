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
        PlaytimeInit = Init(EProperties.PlayTime,Playtime);
        PowderInit = Init(EProperties.Gunpowder,Powder);
        DeclineInit = Init(EProperties.DeclinePowder,Decline);
        LifeInit = Init(EProperties.Life, Life);
    }
    
    private int Init(EProperties properties, UI_RoomSetupButton button)
    {
        Room currentRoom = PhotonNetwork.CurrentRoom;
        
        
        Debug.Log(currentRoom.CustomProperties.ToString());
        Debug.Log(currentRoom.CustomProperties[properties.ToString()]);
        Debug.Log(currentRoom.CustomProperties[properties.ToString()].GetType());
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
            {$"{EProperties.PlayTime}", Playtime.Value.text},
            {$"{EProperties.Life}", Life.Value.text},
            {$"{EProperties.Gunpowder}", Powder.Value.text},
            {$"{EProperties.DeclinePowder}", Decline.Value.text},
        };
        
        currentRoom.SetCustomProperties(roomProperties);
        
    }

    public void CancelButton()
    {
        SetupButtonInit(Playtime, PlaytimeInit);
        SetupButtonInit(Powder, PowderInit);
        SetupButtonInit(Decline, DeclineInit);
        SetupButtonInit(Life, LifeInit);
    }

    private void SetupButtonInit(UI_RoomSetupButton button, int value)
    {
        button.InitValue = value;
        button.Init();
    }
}
