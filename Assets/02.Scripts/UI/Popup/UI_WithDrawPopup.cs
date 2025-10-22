
public class UI_WithdrawPopup : UI_Popup
{
    public void OnClickCloseButton()
    {
        Close();
    }

    public void OnClickConfirmButton()
    {
        AccountManager.Instance.DeleteAccount();
        Close();
    }
}
