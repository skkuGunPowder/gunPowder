[System.Serializable]
public class ChatMessageData
{
    public string type;      // "chat", "whisper", "invite", "data", "system"
    public string content;   // 실제 메시지 텍스트
    public string outfit;    // 쉼표 구분 아웃핏 ID (예: "Hair_01,Face_02,Body_01")
}
