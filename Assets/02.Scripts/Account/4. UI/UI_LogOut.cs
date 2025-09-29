using UnityEngine;
using Photon.Pun;

public class UI_LogOut : MonoBehaviour
{
	[Header("씬 전환")]
	public string LogoutRedirectSceneName = "Photon";

    // 일반 로그아웃 버튼에 연결
	public void OnClickLogout()
	{
		// Photon 연결 해제
		PhotonNetwork.Disconnect();

		// 계정 상태 초기화
		AccountManager.Instance.Logout();;

		// 씬 전환
		if (!string.IsNullOrEmpty(LogoutRedirectSceneName))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(LogoutRedirectSceneName);
		}
	}
}
