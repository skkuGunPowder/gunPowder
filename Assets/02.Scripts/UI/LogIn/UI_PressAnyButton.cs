using UnityEngine;

public class UI_PressAnyButton : MonoBehaviour
{
    public GameObject LoginAndRegisterPanel;
    public void OnClick()
    {
        // 사운드 재생

        // 패널 활성화
        LoginAndRegisterPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
