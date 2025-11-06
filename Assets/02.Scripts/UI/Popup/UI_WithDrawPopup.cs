
public class UI_WithdrawPopup : UI_Popup
{
    private string LogoutRedirectSceneName = "Photon";

    public void OnClickCloseButton()
    {
        Close();
    }

    public void OnClickConfirmButton()
    {
        AccountManager.Instance.DeleteAccount();
        AccountManager.Instance.Logout();;

		if (!string.IsNullOrEmpty(LogoutRedirectSceneName))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(LogoutRedirectSceneName);
		}
        Close();
    }
}
