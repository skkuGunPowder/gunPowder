
public class UI_WithDrawPopup : UI_Popup
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
