using UnityEngine;
using UnityEngine.UI; // TextMeshPro를 쓰신다면 TMPro로 변경
using TMPro; 

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText; // 혹은 Text
    [SerializeField] private float lifeTime = 3.0f;

    public void Setup(string text)
    {
        messageText.text = text;
        Invoke("DestroyBubble", lifeTime); // 3초 뒤 삭제 예약
    }

    private void DestroyBubble()
    {
        Destroy(gameObject);
    }
}