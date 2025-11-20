using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackndChat;

/// <summary>
/// 채팅 메시지 슬롯 (재사용 가능)
/// </summary>
public class UI_ChatMessageSlot : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI SenderNameText;
    public TextMeshProUGUI MessageText;
    public TextMeshProUGUI TimeText;
    public Image ProfileImage;

    [Header("Layouts")]
    public GameObject MyMessageLayout;      // 내 메시지 레이아웃 (오른쪽 정렬)
    public GameObject OtherMessageLayout;   // 상대방 메시지 레이아웃 (왼쪽 정렬)

    [Header("Colors")]
    public Color MyMessageColor = new Color(0.7f, 0.9f, 1f);       // 연한 파란색
    public Color OtherMessageColor = new Color(0.95f, 0.95f, 0.95f); // 연한 회색

    /// <summary>
    /// 메시지 설정
    /// </summary>
    public void SetMessage(MessageInfo messageInfo, bool isMyMessage)
    {
        if (messageInfo == null) return;

        // 레이아웃 활성화/비활성화
        if (MyMessageLayout != null)
            MyMessageLayout.SetActive(isMyMessage);

        if (OtherMessageLayout != null)
            OtherMessageLayout.SetActive(!isMyMessage);

        // 발신자 이름 (내 메시지인 경우 숨김)
        if (SenderNameText != null)
        {
            SenderNameText.text = isMyMessage ? "" : messageInfo.GamerName;
            SenderNameText.gameObject.SetActive(!isMyMessage);
        }

        // 메시지 내용
        if (MessageText != null)
        {
            MessageText.text = messageInfo.Message;

            // 배경색 변경
            var messageBackground = MessageText.GetComponentInParent<Image>();
            if (messageBackground != null)
            {
                messageBackground.color = isMyMessage ? MyMessageColor : OtherMessageColor;
            }
        }

        // 시간 표시
        if (TimeText != null)
        {
            TimeText.text = FormatTime(messageInfo.Time);
        }

        // 프로필 이미지 (TODO: 실제 이미지 로드)
        if (ProfileImage != null && !isMyMessage)
        {
            // TODO: Avatar에 따라 이미지 로드
            // ProfileImage.sprite = Resources.Load<Sprite>($"Avatars/{messageInfo.Avatar}");
        }
    }

    /// <summary>
    /// 시간 포맷팅 (HH:mm)
    /// </summary>
    private string FormatTime(string timeString)
    {
        if (string.IsNullOrEmpty(timeString)) return "";

        try
        {
            // "yyyy-MM-dd HH:mm:ss" → "HH:mm"
            var dateTime = System.DateTime.Parse(timeString);
            return dateTime.ToString("HH:mm");
        }
        catch
        {
            return timeString;
        }
    }

    /// <summary>
    /// 시스템 메시지 설정
    /// </summary>
    public void SetSystemMessage(string message)
    {
        // 레이아웃 모두 비활성화
        if (MyMessageLayout != null)
            MyMessageLayout.SetActive(false);

        if (OtherMessageLayout != null)
            OtherMessageLayout.SetActive(false);

        // 메시지만 중앙 정렬로 표시
        if (MessageText != null)
        {
            MessageText.text = message;
            MessageText.alignment = TextAlignmentOptions.Center;
            MessageText.color = Color.gray;
        }

        // 나머지 UI 숨김
        if (SenderNameText != null)
            SenderNameText.gameObject.SetActive(false);

        if (TimeText != null)
            TimeText.gameObject.SetActive(false);

        if (ProfileImage != null)
            ProfileImage.gameObject.SetActive(false);
    }
}
