using UnityEngine;

public class UI_PressAnyButton : MonoBehaviour
{
    public GameObject LoginAndRegisterPanel;
    public void OnClick()
    {
        // 사운드 재생

        Activate();
    }

    void Update()
    {
        // 이미 활성화되었으면 입력 체크 불필요
        if (LoginAndRegisterPanel != null && LoginAndRegisterPanel.activeSelf)
        {
            return;
        }

        // 키보드/마우스 버튼 입력
        if (Input.anyKeyDown)
        {
            Activate();
            return;
        }

        // 마우스 클릭 별도 체크 (일부 플랫폼에서 anyKeyDown 포함되지 않을 수 있음)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            Activate();
            return;
        }

        // 터치 입력 (모바일)
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Activate();
            }
        }
    }

    private void Activate()
    {
        if (LoginAndRegisterPanel != null)
        {
            LoginAndRegisterPanel.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
