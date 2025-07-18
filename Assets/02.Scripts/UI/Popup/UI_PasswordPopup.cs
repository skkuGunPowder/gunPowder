using UnityEngine;
using UnityEngine.UI;

public class UI_PasswordPopup : UI_Popup
{
    public InputField PasswordInputField;
    
    public bool PasswordCheck(string password)
    {
        if (PasswordInputField.text != password)
        {
            return false;
        }
        
        return true;
    }
}
