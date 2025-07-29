using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ChangeInput : MonoBehaviour
{
    public List<Selectable> navigationOrder; // 원하는 순서대로 등록 (Inspector에서)
    private int currentIndex = 0;

    private void Start()
    {
        if (navigationOrder == null || navigationOrder.Count == 0)
            return;

        EventSystem.current.SetSelectedGameObject(navigationOrder[0].gameObject);
    }

    private void Update()
    {
        if (navigationOrder == null || navigationOrder.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            currentIndex += isShift ? -1 : 1;
            if (currentIndex < 0)
                currentIndex = navigationOrder.Count - 1;
            else if (currentIndex >= navigationOrder.Count)
                currentIndex = 0;

            EventSystem.current.SetSelectedGameObject(navigationOrder[currentIndex].gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var current = navigationOrder[currentIndex];
            Button button = current.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }
    }
}
