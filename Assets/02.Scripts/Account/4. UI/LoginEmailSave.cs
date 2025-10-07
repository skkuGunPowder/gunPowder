using UnityEngine;
using TMPro;

public class LoginEmailSave : MonoBehaviour
{
	[SerializeField] private TMP_InputField emailInputField;

	private const string PlayerPrefsKey = "SavedLoginEmail";

    void Awake()
    {
		if (emailInputField == null)
		{
			return;
		}

		if (PlayerPrefs.HasKey(PlayerPrefsKey))
		{
			string savedEmail = PlayerPrefs.GetString(PlayerPrefsKey);
			emailInputField.text = savedEmail;
		}
		else
		{
			emailInputField.text = string.Empty;
		}
	}
}
