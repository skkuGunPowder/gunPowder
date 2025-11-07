using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UI_PasswordPopup : UI_Popup
{
    public TMP_InputField PasswordInputField;
    private RoomInfo _currentRoomInfo;
    private string _tempPassword;
    
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _currentRoomInfo = roomInfo;
        _tempPassword = _currentRoomInfo.CustomProperties[ERoomProperties.Password.ToString()].ToString();
    }
    
    // 비밀번호 확인해서 적용시킴
    public void PasswordCheck()
    {
        if (PasswordInputField.text == "")
        {
            Fail();
            return;
        }
        
        if (PasswordInputField.text != _tempPassword)
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
