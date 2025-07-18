using Photon.Pun;
using TMPro;
public class UI_PasswordPopup : UI_Popup
{
    public TMP_InputField PasswordInputField;
    
    // 비밀번호 확인해서 적용시킴
    public void PasswordCheck(string password, string roomName)
    {
        if (PasswordInputField.text != password)
        {
            Fail();
        }
        
        Success(roomName); 
    }

    private void Fail()
    {
        
    }

    private void Success(string roomName)
    {
        Close();
        PhotonNetwork.JoinRoom(roomName);
    }
}
