# 채팅 메시지 JSON 포맷 전환 작업 계획서

## 1. 목적

현재 채팅 메시지는 `$$DATA$$` 구분자로 텍스트와 아웃핏 데이터를 이어붙인 평문 문자열로 통신하고 있다.
이 방식은 다음과 같은 문제를 갖고 있다:

- 사용자가 메시지에 `$$DATA$$`를 입력하면 파싱이 깨짐
- 아웃핏 데이터를 조작하여 다른 사람의 스킨으로 위장 가능
- 위스퍼 메시지에서 `!partyinvite` 접두어로 메시지 타입을 구분하는데, 일반 대화에 이 접두어를 넣으면 파티 초대로 오인됨
- 친구 기능 등 새로운 데이터 필드가 추가될 때마다 구분자를 늘려야 해서 확장성이 떨어짐

**→ JSON 포맷으로 전환하여 구조화된 메시지 통신 체계를 만든다.**

---

## 2. 현재 메시지 흐름 분석

### 2-1. 인게임 채팅 (일반 메시지)
```
[송신] SendChatMessage(text)
  → text + "$$DATA$$" + outfitString
  → ChatClient.SendChatMessage(group, name, number, finalMessage)

[수신] OnChatMessage(MessageInfo)
  → "$$DATA$$"로 split
  → parts[0] → messageInfo.Message에 덮어쓰기
  → parts[1] → _messageOutfits 딕셔너리에 저장
  → 채널 타입별 이벤트 발행 (OnChatMessageReceived / OnPartyChatReceived / OnFriendChatReceived)
```

### 2-2. 위스퍼 (귓속말 + 파티 초대)
```
[송신 - 귓속말] SendChatMessage("/w nickname message")
  → ChatClient.SendWhisperMessage(nickname, message)

[송신 - 파티 초대] SendPartyInvite(friendNickname, partyId)
  → "!partyinvite {partyId}"
  → ChatClient.SendWhisperMessage(friendNickname, message)

[수신] OnWhisperMessage(WhisperMessageInfo)
  → "!partyinvite "로 시작하면 → OnPartyInviteReceived 이벤트
  → 아니면 → "[귓속말] " 접두어 붙여서 OnChatMessageReceived 이벤트
```

### 2-3. 파티/친구 채팅
```
[파티] SendPartyMessage(partyId, text)
  → SendMessageToChannel("party", "party_{partyId}", hash, text)
  → 아웃핏 정보 없이 평문만 전송

[친구] SendFriendMessage(myUid, friendUid, text)
  → SendMessageToChannel("friend", channelName, 0, text)
  → 아웃핏 정보 없이 평문만 전송
```

---

## 3. JSON 메시지 구조 설계

### 3-1. 데이터 클래스 (`ChatMessageData.cs` 신규 생성)

```csharp
// Assets/02.Scripts/Chat/ChatMessageData.cs

[System.Serializable]
public class ChatMessageData
{
    public string type;      // "chat", "whisper", "partyInvite", "friendRequest" 등
    public string content;   // 실제 메시지 텍스트
    public string outfit;    // 쉼표 구분 아웃핏 ID (예: "Hair_01,Face_02,Body_01")
    // 향후 확장 필드 (친구 요청, 이모션, 시스템 메시지 등)
}
```

> `JsonUtility.ToJson()` / `JsonUtility.FromJson<T>()` 사용 (Unity 내장, 추가 패키지 불필요)

### 3-2. 메시지 타입 정의

| type 값 | 용도 | 포함 데이터 |
|---|---|---|
| `"chat"` | 일반 채팅 (인게임/파티/친구) | content, outfit |
| `"whisper"` | 귓속말 | content |
| `"partyInvite"` | 파티 초대 | content(=partyId) |
| (향후) `"friendRequest"` | 친구 요청 | content(=요청 메시지) |
| (향후) `"system"` | 시스템 알림 | content |

---

## 4. 수정 대상 파일 및 작업 내용

### 4-1. 신규 파일

| 파일 | 설명 |
|---|---|
| `Assets/02.Scripts/Chat/ChatMessageData.cs` | JSON 직렬화용 데이터 클래스 |

### 4-2. `UIChatManager.cs` 수정

#### (A) 상수 및 필드
- `SPLIT_TAG` 상수 제거 (더 이상 사용하지 않음)
- `_messageOutfits` 딕셔너리는 유지 (수신 측 outfit 캐싱 용도)

#### (B) `SendChatMessage(string text)` (125번째 줄)
**변경 전:**
```csharp
string myOutfit = ItemStorage.Instance.GetMyOutfitString();
string finalMessage = $"{text}{SPLIT_TAG}{myOutfit}";
_chatClient.SendChatMessage(..., finalMessage);
```
**변경 후:**
```csharp
ChatMessageData data = new ChatMessageData
{
    type = "chat",
    content = text,
    outfit = ItemStorage.Instance.GetMyOutfitString()
};
string finalMessage = JsonUtility.ToJson(data);
_chatClient.SendChatMessage(..., finalMessage);
```

#### (C) `OnChatMessage(MessageInfo)` (401번째 줄)
**변경 전:** `$$DATA$$`로 split하여 파싱
**변경 후:**
```csharp
ChatMessageData data = JsonUtility.FromJson<ChatMessageData>(messageInfo.Message);
if (data != null && !string.IsNullOrEmpty(data.type))
{
    messageInfo.Message = data.content;
    if (!string.IsNullOrEmpty(data.outfit))
    {
        _messageOutfits[messageInfo.Index] = new List<string>(data.outfit.Split(','));
    }
}
// else: JSON 파싱 실패 시 기존 평문으로 간주 (하위 호환)
```

#### (D) `SendPartyInvite(string friendNickname, string partyId)` (1076번째 줄)
**변경 전:**
```csharp
string message = $"!partyinvite {partyId}";
_chatClient.SendWhisperMessage(friendNickname, message);
```
**변경 후:**
```csharp
ChatMessageData data = new ChatMessageData
{
    type = "partyInvite",
    content = partyId
};
_chatClient.SendWhisperMessage(friendNickname, JsonUtility.ToJson(data));
```

#### (E) `OnWhisperMessage(WhisperMessageInfo)` (482번째 줄)
**변경 전:** `"!partyinvite "` 접두어로 분기
**변경 후:**
```csharp
ChatMessageData data = JsonUtility.FromJson<ChatMessageData>(messageInfo.Message);
if (data != null && data.type == "partyInvite")
{
    OnPartyInviteReceived?.Invoke(messageInfo.FromGamerName, data.content);
    return;
}
// 일반 귓속말 처리 (data.type == "whisper" 또는 하위 호환 평문)
string whisperText = (data != null) ? data.content : messageInfo.Message;
// ... "[귓속말] " 접두어 붙여서 처리
```

#### (F) `SendMessageToChannel(...)` (1094번째 줄)
파티/친구 채팅도 JSON으로 통일:
```csharp
// SendPartyMessage, SendFriendMessage에서 호출 시
// text를 그대로 보내지 않고 ChatMessageData로 감싸서 전송
ChatMessageData data = new ChatMessageData
{
    type = "chat",
    content = text,
    outfit = ItemStorage.Instance.GetMyOutfitString()
};
_chatClient.SendChatMessage(channelGroup, channelName, channelNumber, JsonUtility.ToJson(data));
```

### 4-3. 외부 파일 (변경 없음, 호환 확인만)

| 파일 | 이유 |
|---|---|
| `UI_IngameChatPopup.cs` | `GetMessageOutfit()`으로 outfit을 가져오는 방식 그대로 유지 → 변경 불필요 |
| `UI_PartyChatPanel.cs` | `UIChatManager.SendPartyMessage()`를 호출만 함 → 변경 불필요 |
| `UI_FriendChatPanel.cs` | `UIChatManager.SendFriendMessage()`를 호출만 함 → 변경 불필요 |
| `UIChatList.cs` | `messageInfo.Message`에서 이미 파싱된 텍스트를 받음 → 변경 불필요 |
| `LobbyChatController.cs` | `messageInfo.Message`를 표시만 함 → 변경 불필요 |

---

## 5. 작업 순서

```
Step 1. ChatMessageData.cs 생성
         → [Serializable] 클래스 작성

Step 2. UIChatManager.cs 송신 로직 수정
         → SendChatMessage() — JSON 직렬화
         → SendPartyInvite() — JSON 직렬화
         → SendMessageToChannel() — JSON 직렬화

Step 3. UIChatManager.cs 수신 로직 수정
         → OnChatMessage() — JSON 역직렬화 + 하위 호환 fallback
         → OnWhisperMessage() — JSON 역직렬화 + 하위 호환 fallback

Step 4. SPLIT_TAG 상수 제거
         → 하위 호환 fallback에서도 사용하지 않으면 제거
         → 하위 호환을 유지하려면 fallback용으로 남겨둘 수 있음

Step 5. 테스트
         → ParrelSync로 2개 에디터 실행
         → 인게임 채팅: 메시지 송수신 + 아웃핏 표시 확인
         → 파티 채팅: 메시지 송수신 확인
         → 파티 초대: 위스퍼로 초대 전송/수신 확인
         → 귓속말: /w 명령어 동작 확인
         → 엣지 케이스: 특수문자, 빈 메시지, 긴 메시지 테스트
```

---

## 6. 하위 호환성 전략

수신 측에서 JSON 파싱을 먼저 시도하고, 실패 시 기존 `$$DATA$$` 포맷으로 fallback 처리한다.
모든 클라이언트가 업데이트된 후 fallback 코드를 제거할 수 있다.

```csharp
// 수신 시 파싱 우선순위
1. JsonUtility.FromJson<ChatMessageData>() 시도
2. 실패 시 → $$DATA$$ split으로 fallback
3. 그것도 없으면 → 평문 메시지로 처리
```

---

## 7. 향후 확장 가능성

JSON 포맷 전환 후 `ChatMessageData`에 필드를 추가하는 것만으로 새로운 기능을 지원할 수 있다:

```csharp
// 예시: 친구 요청 기능 추가 시
public class ChatMessageData
{
    public string type;       // "friendRequest"
    public string content;    // 요청 메시지
    public string outfit;     // 아웃핏
    public string senderId;   // 요청자 UID (신규)
}
```

기존 클라이언트는 알 수 없는 `type`을 무시하면 되므로, 하위 호환을 깨뜨리지 않는다.
