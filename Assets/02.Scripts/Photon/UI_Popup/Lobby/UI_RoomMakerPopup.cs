using TMPro;
using UnityEngine.UI;

public class UI_RoomMakerPopup : UI_Popup
{
    public TMP_InputField RoomName;
    public TMP_InputField RoomPassword;
    public string BlankRoomName;
    public int MaxPlayerCount;
    // public 
    public Toggle IsLocked;            // 비번 방 여부
    // public 
    public Toggle[] MaxPlayers;
    public UI_RoomSetupButton PlayTime;
    public UI_RoomSetupButton Life;
    public UI_RoomSetupButton Gunpowder;

    private void Start()
    {
        OnClickMaxPlayer(0);
    }

    private void OnEnable()
    {
        PlayTime.Init();
        Life.Init();
        Gunpowder.Init();
    }

    public void OnclickCreateRoom()
    {
        string roomName = RoomName.text;
       
        if (roomName == "")
        {
            roomName = BlankRoomName;
        }
        
        else if (roomName.Length < 3)
        {
            UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
            //popup.Init("방 이름은 3글자 이상이어야합니다.", false);
            popup.Init("ルーム名は3文字以上で入力してください。", false);
            return;
        }
        
        if (IsLocked.isOn == false)
        {
            RoomPassword.text = "";
        }
        
        LobbyManager.Instance.MakeRoom(roomName, MaxPlayerCount, PlayTime.CurrentValue(), Life.CurrentValue(),
            Gunpowder.CurrentValue(), IsLocked.isOn, RoomPassword.text);
        
        Close();
    }
    
    // UI 초기화하기
    public void OnDisable()
    {
        IsLocked.isOn = false;
        RoomName.text = "";
        RoomPassword.text = "";
        OnClickMaxPlayer(0);
    }
    
    public void OnClickMaxPlayer(int index)
    {
        if (!MaxPlayers[index].isOn) return;

        MaxPlayerCount = index + 2;
    }
    public void LockedButton()
    {
        RoomPassword.interactable = IsLocked.isOn;
    }
}
