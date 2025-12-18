# 클래스 관계도 및 의존성 분석

## 1. 전체 클래스 관계도

```mermaid
classDiagram
    class MonoBehaviour {
        <<Unity>>
    }
    
    class Singleton~T~ {
        <<abstract>>
        +static T Instance
        #Awake()
    }
    
    class MonoBehaviourPunCallbacks {
        <<Photon>>
    }
    
    class UI_Popup {
        <<abstract>>
        -Action _closeCallback
        +Open(Action)
        +Close()
    }
    
    class PopupManager {
        -List~UI_Popup~ PopupList
        -Stack~UI_Popup~ _popupStack
        -UI_IngameChatPopup _ingameChatPopup
        +Open(EPopupType, Action)
        +Close(EPopupType)
        -PopupOpen(string, Action)
        -PopupClose(string)
    }
    
    class PopupSlot {
        +EPopupType Popup
        -Button _button
        +Open()
        +Close()
    }
    
    class UI_MessagePopup {
        +Button OKButton
        +Button CancleButton
        +TextMeshProUGUI MessageText
        -Action _callback
        +Init(string, bool, Action)
        +SetText(string)
        +SetCallback(Action)
        +OnClickOK()
        +OnClickCancle()
    }
    
    class UI_InformationPopup {
        -TextMeshProUGUI _emailText
        -TextMeshProUGUI _tagText
        -TextMeshProUGUI _nicknameText
        -TMP_InputField _nicknameInputField
        +Refresh()
        +OnClickPasswordChange()
        +OnClickDeleteAccount()
        +OnClickChangeNickname()
        +OnSetNickname()
    }
    
    class UI_WithdrawPopup {
        -string LogoutRedirectSceneName
        +OnClickCloseButton()
        +OnClickConfirmButton()
    }
    
    class UI_MenuPopup {
    }
    
    class UI_GameStartPopup {
    }
    
    class UI_PasswordPopup {
        +TMP_InputField PasswordInputField
        -RoomInfo _currentRoomInfo
        -string _tempPassword
        +SetRoomInfo(RoomInfo)
        +PasswordCheck()
        -Fail()
        -Success(string)
    }
    
    class UI_PasswordWrongPopup {
    }
    
    class UI_RoomMakerPopup {
        +TMP_InputField RoomName
        +TMP_InputField RoomPassword
        +string BlankRoomName
        +int MaxPlayerCount
        +Toggle IsLocked
        +Toggle[] MaxPlayers
        +UI_RoomSetupButton PlayTime
        +UI_RoomSetupButton Life
        +UI_RoomSetupButton Gunpowder
        +UI_RoomSetupButton Decline
        +OnclickCreateRoom()
        +OnClickMaxPlayer(int)
        +LockedButton()
    }
    
    class UI_RoomSearchPopup {
        +List~UI_RoomSlot~ RoomSlotList
        +TextMeshProUGUI RoomPageTextUGUI
        -int _currentPage
        -int _maxPage
        +Refresh()
        +OnClickSetPage(int)
        -StringToSprite(RoomInfo)
        -PageSetting()
    }
    
    class UI_RoomSetupButton {
        +TextMeshProUGUI Value
        +int MaxValue
        +int MinValue
        +int InitValue
        -int _currentValue
        +Init()
        +ValueUpDown(int)
        +Refresh()
        +Reset(int)
        +CurrentValue() int
    }
    
    class PhotonServerManager {
        +static PhotonServerManager Instance
        -int _sendRate
        -int _serializationRate
        -string _gameVersion
        -List~RoomInfo~ _roomInfoList
        -bool _isTutorial
        -bool _isFirst
        +Connect(bool)
        +TutorialMode()
        +SetPhotonPrefabPool(Dictionary)
        +SetFirst(bool)
        +OnConnected()
        +OnDisconnected(DisconnectCause)
        +OnConnectedToMaster()
        +OnJoinedLobby()
        +OnJoinRandomFailed(short, string)
        +OnJoinedRoom()
        +OnCreatedRoom()
        +OnPlayerLeftRoom(PhotonPlayer)
        +OnRoomListUpdate(List~RoomInfo~)
    }
    
    class AccountManager {
        <<Singleton>>
        +CurrentAccount
        +ChangePassword()
        +SetNickname(string) Result
        +DeleteAccount()
        +Logout()
    }
    
    class FirebaseManager {
        +Auth
    }
    
    class CurrencyManager {
        <<Singleton>>
        +SubtractCurrency(ECurrencyType, int)
    }
    
    class LobbyManager {
        <<Singleton>>
        +MakeRoom(string, int, int, int, int, int, bool, string)
    }
    
    class EventManager {
        <<Singleton>>
        +OnRoomListUpdate event
        +PlayerLeft(PhotonPlayer)
        +PlayerLeftRoom(PhotonPlayer)
        +PlayerFind()
        +RoomListUpdate()
    }
    
    class MapDataManager {
        <<Singleton>>
        +GetThemeData(EMap) MapThemeData
    }
    
    class TransitionManager {
        <<Singleton>>
        +LoadLevel(ESceneList)
    }
    
    %% 상속 관계
    Singleton~T~ <|-- PopupManager : extends
    MonoBehaviour <|-- Singleton~T~ : extends
    MonoBehaviour <|-- PopupSlot : extends
    MonoBehaviour <|-- UI_Popup : extends
    MonoBehaviourPunCallbacks <|-- PhotonServerManager : extends
    
    UI_Popup <|-- UI_MessagePopup : extends
    UI_Popup <|-- UI_InformationPopup : extends
    UI_Popup <|-- UI_WithdrawPopup : extends
    UI_Popup <|-- UI_MenuPopup : extends
    UI_Popup <|-- UI_GameStartPopup : extends
    UI_Popup <|-- UI_PasswordPopup : extends
    UI_Popup <|-- UI_PasswordWrongPopup : extends
    UI_Popup <|-- UI_RoomMakerPopup : extends
    UI_Popup <|-- UI_RoomSearchPopup : extends
    
    %% 의존성 관계
    PopupSlot --> PopupManager : uses Instance
    PopupManager --> UI_Popup : manages List
    PopupManager --> UI_IngameChatPopup : caches reference
    
    UI_InformationPopup --> PopupManager : uses Instance
    UI_InformationPopup --> AccountManager : uses Instance
    UI_InformationPopup --> FirebaseManager : uses Instance
    UI_InformationPopup --> CurrencyManager : uses Instance
    UI_InformationPopup --> UI_MessagePopup : creates
    
    UI_WithdrawPopup --> AccountManager : uses Instance
    
    UI_MessagePopup --> PopupManager : used by
    
    PhotonServerManager --> PopupManager : uses Instance
    PhotonServerManager --> AccountManager : uses Instance
    PhotonServerManager --> TransitionManager : uses Instance
    PhotonServerManager --> EventManager : uses Instance
    PhotonServerManager --> LobbyManager : uses Instance
    PhotonServerManager --> UI_MessagePopup : creates
    
    UI_PasswordPopup --> PopupManager : uses Instance
    UI_PasswordPopup --> PhotonNetwork : uses
    
    UI_RoomMakerPopup --> PopupManager : uses Instance
    UI_RoomMakerPopup --> LobbyManager : uses Instance
    UI_RoomMakerPopup --> UI_MessagePopup : creates
    UI_RoomMakerPopup --> UI_RoomSetupButton : contains 4x
    
    UI_RoomSearchPopup --> PhotonServerManager : uses Instance
    UI_RoomSearchPopup --> EventManager : subscribes to
    UI_RoomSearchPopup --> MapDataManager : uses Instance
    UI_RoomSearchPopup --> UI_RoomSlot : contains List
```

## 2. 팝업 시스템 구조

```mermaid
graph TB
    subgraph "팝업 관리 계층"
        PM[PopupManager<br/>Singleton]
        PS[PopupSlot<br/>버튼 컴포넌트]
    end
    
    subgraph "팝업 베이스 클래스"
        UP[UI_Popup<br/>abstract class]
    end
    
    subgraph "정보 관련 팝업"
        IP[UI_InformationPopup]
        WP[UI_WithdrawPopup]
        MP[UI_MenuPopup]
    end
    
    subgraph "메시지 팝업"
        MSP[UI_MessagePopup]
    end
    
    subgraph "로비 관련 팝업"
        RMP[UI_RoomMakerPopup]
        RSP[UI_RoomSearchPopup]
        PP[UI_PasswordPopup]
        PWP[UI_PasswordWrongPopup]
        GSP[UI_GameStartPopup]
    end
    
    subgraph "외부 매니저"
        AM[AccountManager]
        FM[FirebaseManager]
        CM[CurrencyManager]
        LM[LobbyManager]
        PSM[PhotonServerManager]
        EM[EventManager]
        MDM[MapDataManager]
    end
    
    subgraph "UI 컴포넌트"
        RSB[UI_RoomSetupButton]
    end
    
    PM -->|관리| UP
    PS -->|호출| PM
    UP -->|상속| IP
    UP -->|상속| WP
    UP -->|상속| MP
    UP -->|상속| MSP
    UP -->|상속| RMP
    UP -->|상속| RSP
    UP -->|상속| PP
    UP -->|상속| PWP
    UP -->|상속| GSP
    
    IP -->|사용| PM
    IP -->|사용| AM
    IP -->|사용| FM
    IP -->|사용| CM
    IP -->|생성| MSP
    
    WP -->|사용| AM
    
    RMP -->|사용| PM
    RMP -->|사용| LM
    RMP -->|생성| MSP
    RMP -->|포함| RSB
    
    RSP -->|사용| PSM
    RSP -->|구독| EM
    RSP -->|사용| MDM
    
    PP -->|사용| PM
    
    PSM -->|사용| PM
    PSM -->|사용| AM
    PSM -->|사용| LM
    PSM -->|생성| MSP
    
    style PM fill:#4CAF50
    style UP fill:#2196F3
    style MSP fill:#FF9800
```

## 3. 의존성 흐름도

```mermaid
graph LR
    subgraph "사용자 입력"
        Button[버튼 클릭]
        Keyboard[키보드 입력]
    end
    
    subgraph "UI 레이어"
        PopupSlot[PopupSlot]
        PopupManager[PopupManager]
    end
    
    subgraph "팝업 레이어"
        InfoPopup[UI_InformationPopup]
        MessagePopup[UI_MessagePopup]
        RoomMaker[UI_RoomMakerPopup]
        RoomSearch[UI_RoomSearchPopup]
        PasswordPopup[UI_PasswordPopup]
    end
    
    subgraph "비즈니스 로직 레이어"
        AccountMgr[AccountManager]
        LobbyMgr[LobbyManager]
        PhotonMgr[PhotonServerManager]
        EventMgr[EventManager]
    end
    
    subgraph "데이터 레이어"
        Firebase[FirebaseManager]
        Currency[CurrencyManager]
        MapData[MapDataManager]
    end
    
    Button --> PopupSlot
    Keyboard --> PopupManager
    PopupSlot --> PopupManager
    PopupManager --> InfoPopup
    PopupManager --> MessagePopup
    PopupManager --> RoomMaker
    PopupManager --> RoomSearch
    PopupManager --> PasswordPopup
    
    InfoPopup --> AccountMgr
    InfoPopup --> Firebase
    InfoPopup --> Currency
    InfoPopup --> MessagePopup
    
    RoomMaker --> LobbyMgr
    RoomMaker --> MessagePopup
    
    RoomSearch --> PhotonMgr
    RoomSearch --> EventMgr
    RoomSearch --> MapData
    
    PasswordPopup --> PhotonMgr
    
    PhotonMgr --> AccountMgr
    PhotonMgr --> LobbyMgr
    PhotonMgr --> EventMgr
    PhotonMgr --> MessagePopup
    
    style PopupManager fill:#4CAF50
    style AccountMgr fill:#FF9800
    style PhotonMgr fill:#2196F3
```

## 4. PhotonServerManager 의존성 상세

```mermaid
graph TD
    PSM[PhotonServerManager]
    
    subgraph "직접 의존"
        PM[PopupManager.Instance]
        AM[AccountManager.Instance]
        TM[TransitionManager.Instance]
        EM[EventManager.Instance]
        LM[LobbyManager.Instance]
    end
    
    subgraph "간접 의존"
        MSP[UI_MessagePopup]
        PNW[PhotonNetwork]
        SM[SceneManager]
    end
    
    subgraph "콜백 체인"
        OC[OnConnected]
        OCM[OnConnectedToMaster]
        OJL[OnJoinedLobby]
        OJR[OnJoinedRoom]
        OCR[OnCreatedRoom]
        OJRF[OnJoinRandomFailed]
        OPLR[OnPlayerLeftRoom]
        ORLU[OnRoomListUpdate]
    end
    
    PSM --> PM
    PSM --> AM
    PSM --> TM
    PSM --> EM
    PSM --> LM
    
    PSM --> OJRF
    OJRF --> PM
    OJRF --> MSP
    OJRF --> LM
    
    PSM --> OCR
    OCR --> PM
    OCR --> TM
    
    PSM --> ORLU
    ORLU --> EM
    
    PSM --> OPLR
    OPLR --> EM
    
    style PSM fill:#2196F3
    style PM fill:#4CAF50
    style EM fill:#FF9800
```

## 5. 팝업 간 상호작용 관계

```mermaid
graph TD
    subgraph "팝업 열기 트리거"
        PS[PopupSlot.Open]
        PM[PopupManager.Open]
        Direct[직접 호출]
    end
    
    subgraph "정보 팝업 그룹"
        IP[UI_InformationPopup]
        IP -->|OnClickDeleteAccount| WP[UI_WithdrawPopup]
        IP -->|OnClickPasswordChange| MSP1[UI_MessagePopup<br/>비밀번호 변경 완료]
        IP -->|OnClickChangeNickname| MSP2[UI_MessagePopup<br/>닉네임 변경 확인]
        IP -->|OnSetNickname| MSP3[UI_MessagePopup<br/>성공/실패]
    end
    
    subgraph "방 관련 팝업 그룹"
        RMP[UI_RoomMakerPopup]
        RMP -->|OnclickCreateRoom<br/>실패 시| MSP4[UI_MessagePopup<br/>방 이름 오류]
        RMP -->|성공 시| LM[LobbyManager]
        
        RSP[UI_RoomSearchPopup]
        RSP -->|방 선택 시| PP[UI_PasswordPopup]
        PP -->|비밀번호 틀림| PWP[UI_PasswordWrongPopup]
        PP -->|성공| PNW[PhotonNetwork.JoinRoom]
    end
    
    subgraph "Photon 콜백"
        PSM[PhotonServerManager]
        PSM -->|OnJoinRandomFailed| MSP5[UI_MessagePopup<br/>방 없음]
        MSP5 -->|확인 시| LM
    end
    
    PS --> PM
    Direct --> PM
    PM --> IP
    PM --> WP
    PM --> RMP
    PM --> RSP
    PM --> PP
    PM --> PWP
    PM --> MSP1
    PM --> MSP2
    PM --> MSP3
    PM --> MSP4
    PM --> MSP5
    
    style PM fill:#4CAF50
    style MSP1 fill:#FF9800
    style MSP2 fill:#FF9800
    style MSP3 fill:#FF9800
    style MSP4 fill:#FF9800
    style MSP5 fill:#FF9800
```

## 6. 데이터 흐름도

```mermaid
sequenceDiagram
    participant User as 사용자
    participant PS as PopupSlot
    participant PM as PopupManager
    participant IP as UI_InformationPopup
    participant AM as AccountManager
    participant FM as FirebaseManager
    participant CM as CurrencyManager
    participant MSP as UI_MessagePopup
    
    User->>PS: 버튼 클릭
    PS->>PM: Open(EPopupType.UI_InformationPopup)
    PM->>IP: Open()
    IP->>IP: Refresh()
    IP->>FM: Auth.CurrentUser
    FM-->>IP: 사용자 정보
    IP->>IP: UI 업데이트
    
    User->>IP: 닉네임 변경 클릭
    IP->>PM: Open(UI_MessagePopup)
    PM->>MSP: Open()
    MSP->>MSP: Init(확인 메시지)
    
    User->>MSP: 확인 버튼
    MSP->>IP: OnSetNickname 콜백
    IP->>AM: SetNickname()
    AM-->>IP: Result
    alt 성공
        IP->>CM: SubtractCurrency()
        IP->>PM: Open(UI_MessagePopup)
        PM->>MSP: Open()
        MSP->>MSP: Init(성공 메시지)
    else 실패
        IP->>PM: Open(UI_MessagePopup)
        PM->>MSP: Open()
        MSP->>MSP: Init(실패 메시지)
    end
```

## 7. 컴포넌트 구성 관계

```mermaid
graph TB
    subgraph "UI_RoomMakerPopup 구성"
        RMP[UI_RoomMakerPopup]
        RN[TMP_InputField<br/>RoomName]
        RP[TMP_InputField<br/>RoomPassword]
        IL[Toggle<br/>IsLocked]
        MP[Toggle[]<br/>MaxPlayers]
        PT[UI_RoomSetupButton<br/>PlayTime]
        LF[UI_RoomSetupButton<br/>Life]
        GP[UI_RoomSetupButton<br/>Gunpowder]
        DC[UI_RoomSetupButton<br/>Decline]
    end
    
    subgraph "UI_RoomSetupButton 내부"
        RSB[UI_RoomSetupButton]
        VT[TextMeshProUGUI<br/>Value]
        MV[int MaxValue]
        MN[int MinValue]
        IV[int InitValue]
        CV[int _currentValue]
    end
    
    subgraph "UI_InformationPopup 구성"
        IP[UI_InformationPopup]
        ET[TextMeshProUGUI<br/>_emailText]
        TT[TextMeshProUGUI<br/>_tagText]
        NT[TextMeshProUGUI<br/>_nicknameText]
        NF[TMP_InputField<br/>_nicknameInputField]
    end
    
    subgraph "UI_MessagePopup 구성"
        MSP[UI_MessagePopup]
        OKB[Button<br/>OKButton]
        CB[Button<br/>CancleButton]
        MT[TextMeshProUGUI<br/>MessageText]
        AC[Action<br/>_callback]
    end
    
    RMP --> RN
    RMP --> RP
    RMP --> IL
    RMP --> MP
    RMP --> PT
    RMP --> LF
    RMP --> GP
    RMP --> DC
    
    PT --> RSB
    LF --> RSB
    GP --> RSB
    DC --> RSB
    
    RSB --> VT
    RSB --> MV
    RSB --> MN
    RSB --> IV
    RSB --> CV
    
    IP --> ET
    IP --> TT
    IP --> NT
    IP --> NF
    
    MSP --> OKB
    MSP --> CB
    MSP --> MT
    MSP --> AC
    
    style RMP fill:#4CAF50
    style IP fill:#2196F3
    style MSP fill:#FF9800
    style RSB fill:#9C27B0
```

## 8. 이벤트 구독 관계

```mermaid
graph LR
    subgraph "이벤트 발행자"
        PSM[PhotonServerManager]
        EM[EventManager]
    end
    
    subgraph "이벤트 구독자"
        RSP[UI_RoomSearchPopup]
    end
    
    subgraph "이벤트 타입"
        RLU[OnRoomListUpdate]
    end
    
    PSM -->|OnRoomListUpdate 호출| EM
    EM -->|이벤트 발행| RLU
    RSP -->|구독| RLU
    RLU -->|Refresh 호출| RSP
    
    style EM fill:#FF9800
    style RSP fill:#4CAF50
```

## 주요 관계 요약

### 상속 관계
- `PopupManager` → `Singleton<PopupManager>` → `MonoBehaviour`
- 모든 팝업 클래스 → `UI_Popup` → `MonoBehaviour`
- `PhotonServerManager` → `MonoBehaviourPunCallbacks`

### 의존성 관계
- **PopupSlot** → PopupManager (Instance 사용)
- **모든 팝업** → PopupManager (Instance 사용)
- **UI_InformationPopup** → AccountManager, FirebaseManager, CurrencyManager
- **UI_RoomMakerPopup** → LobbyManager, UI_RoomSetupButton (4개)
- **UI_RoomSearchPopup** → PhotonServerManager, EventManager, MapDataManager
- **PhotonServerManager** → PopupManager, AccountManager, TransitionManager, EventManager, LobbyManager

### 구성 관계
- **UI_RoomMakerPopup**는 4개의 **UI_RoomSetupButton**을 포함
- **UI_MessagePopup**는 Action 콜백을 통해 다른 팝업과 통신

### 이벤트 관계
- **EventManager**는 **UI_RoomSearchPopup**에 이벤트를 발행
- **PhotonServerManager**는 **EventManager**를 통해 이벤트를 전달


