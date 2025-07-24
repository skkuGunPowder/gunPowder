using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UI_PasswordPopup : UI_Popup
{
    public TMP_InputField PasswordInputField;
    public RoomInfo _currentRoomInfo;
    
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _currentRoomInfo = roomInfo;
    }
    
    // 비밀번호 확인해서 적용시킴
    public void PasswordCheck()
    {
        string password = _currentRoomInfo.CustomProperties[$"{EProperties.Password}"].ToString();
            
        if (PasswordInputField.text != password)
        {
            Fail();
            return;
        }
           
        Success(_currentRoomInfo.Name); 
    }

    private void Fail()
    {
        PopupManager.Instance.Open(EPopupType.UI_PasswordWrongPopup);   
    }

    private void Success(string roomName)
    {
        Close();
        PhotonNetwork.JoinRoom(roomName);
    }
}
