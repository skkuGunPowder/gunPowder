# 스크립트 플로우차트 분석

## 1. PhotonServerManager 플로우

```mermaid
flowchart TD
    Start([게임 시작]) --> Awake[Awake: 싱글톤 초기화]
    Awake --> Start[Start: Photon 설정]
    Start --> Connect{Connect 호출}
    
    Connect --> SetVersion[게임 버전 설정]
    SetVersion --> SetNickname[닉네임 설정]
    SetNickname --> ConnectPhoton[PhotonNetwork.ConnectUsingSettings]
    
    ConnectPhoton --> OnConnected[OnConnected]
    OnConnected --> LoadLobby[TransitionManager.LoadLevel: Lobby]
    
    ConnectPhoton --> OnConnectedToMaster[OnConnectedToMaster]
    OnConnectedToMaster --> JoinLobby[PhotonNetwork.JoinLobby]
    
    JoinLobby --> OnJoinedLobby[OnJoinedLobby]
    OnJoinedLobby --> ClearProperties[플레이어 커스텀 프로퍼티 초기화]
    ClearProperties --> RoomListUpdate[OnRoomListUpdate 대기]
    
    Connect --> TutorialMode{TutorialMode 호출?}
    TutorialMode -->|Yes| CreateTutorialRoom[튜토리얼 방 생성]
    CreateTutorialRoom --> OnJoinedRoom[OnJoinedRoom]
    OnJoinedRoom --> IsTutorial{_isTutorial?}
    IsTutorial -->|Yes| LoadTutorial[TransitionManager.LoadLevel: Tutorial]
    IsTutorial -->|No| NormalFlow[정상 플로우]
    
    Connect --> JoinRandom{랜덤 매칭?}
    JoinRandom -->|실패| OnJoinRandomFailed[OnJoinRandomFailed]
    OnJoinRandomFailed --> ShowMessage[UI_MessagePopup: 방 없음 메시지]
    ShowMessage --> MakeRandomRoom[MakeRandomRoom]
    MakeRandomRoom --> CreateRoom[LobbyManager.MakeRoom]
    
    CreateRoom --> OnCreatedRoom[OnCreatedRoom]
    OnCreatedRoom --> IsTutorial2{_isTutorial?}
    IsTutorial2 -->|No| CloseSearchPopup[PopupManager.Close: RoomSearchPopup]
    CloseSearchPopup --> LoadWaitingRoom[TransitionManager.LoadLevel: WaitingRoom]
    
    RoomListUpdate --> UpdateRoomList[방 리스트 업데이트]
    UpdateRoomList --> EventRoomListUpdate[EventManager.RoomListUpdate]
    
    OnPlayerLeftRoom[OnPlayerLeftRoom] --> EventPlayerLeft[EventManager.PlayerLeft]
    EventPlayerLeft --> IsMaster{IsMasterClient?}
    IsMaster -->|Yes| EventPlayerLeftRoom[EventManager.PlayerLeftRoom]
    EventPlayerLeftRoom --> EventPlayerFind[EventManager.PlayerFind]
    
    OnDisconnected[OnDisconnected] --> LoadPhotonScene[SceneManager.LoadScene: Photon]
    
    style Start fill:#e1f5ff
    style LoadLobby fill:#c8e6c9
    style LoadTutorial fill:#c8e6c9
    style LoadWaitingRoom fill:#c8e6c9
    style LoadPhotonScene fill:#ffcdd2
```

## 2. PopupManager 플로우

```mermaid
flowchart TD
    Start([PopupManager 시작]) --> Awake[Awake: 싱글톤 초기화]
    Awake --> CacheChatPopup[인게임 채팅 팝업 캐싱]
    CacheChatPopup --> Update[Update 루프]
    
    Update --> CheckEnter{Enter 키 입력?}
    CheckEnter -->|Yes| TryOpenChat{채팅 팝업 열기 가능?}
    TryOpenChat -->|Yes| OpenChat[Open: UI_IngameChatPopup]
    
    Update --> CheckEscape{Escape 키 입력?}
    CheckEscape -->|Yes| HasPopupStack{팝업 스택 있음?}
    
    HasPopupStack -->|Yes| PopPopup[스택에서 팝업 Pop]
    PopPopup --> IsActive{팝업 활성화?}
    IsActive -->|Yes| ClosePopup[팝업 Close]
    ClosePopup --> Break[루프 종료]
    
    IsActive -->|No| CheckStackEmpty{스택 비어있음?}
    CheckStackEmpty -->|Yes| OpenMenu[Open: UI_MenuPopup]
    CheckStackEmpty -->|No| PopPopup
    
    HasPopupStack -->|No| OpenMenu
    
    Open[Open 호출] --> PopupOpen[PopupOpen]
    PopupOpen --> FindPopup{팝업 찾기}
    FindPopup -->|찾음| ActivatePopup[팝업 활성화]
    ActivatePopup --> PushStack[스택에 Push]
    PushStack --> ReturnPopup[팝업 반환]
    
    FindPopup -->|못찾음| LogError[에러 로그]
    
    Close[Close 호출] --> PopupClose[PopupClose]
    PopupClose --> FindPopupClose{팝업 찾기}
    FindPopupClose -->|찾음| DeactivatePopup[팝업 비활성화]
    
    style Start fill:#e1f5ff
    style OpenChat fill:#c8e6c9
    style OpenMenu fill:#c8e6c9
    style LogError fill:#ffcdd2
```

## 3. 팝업 상호작용 플로우

```mermaid
flowchart TD
    Start([팝업 시스템]) --> PopupSlot[PopupSlot.Open]
    PopupSlot --> PopupManagerOpen[PopupManager.Open]
    PopupManagerOpen --> DisableButton[버튼 비활성화]
    DisableButton --> PopupOpen[UI_Popup.Open]
    PopupOpen --> SetActive[GameObject.SetActive: true]
    SetActive --> CallbackOnClose[닫을 때 콜백 실행]
    
    PopupSlot --> PopupSlotClose[PopupSlot.Close]
    PopupSlotClose --> PopupManagerClose[PopupManager.Close]
    PopupManagerClose --> PopupClose[UI_Popup.Close]
    PopupClose --> InvokeCallback[콜백 실행]
    InvokeCallback --> SetActiveFalse[GameObject.SetActive: false]
    SetActiveFalse --> EnableButton[버튼 활성화]
    
    style Start fill:#e1f5ff
    style SetActive fill:#c8e6c9
    style SetActiveFalse fill:#fff9c4
```

## 4. UI_InformationPopup 플로우

```mermaid
flowchart TD
    Start([정보 팝업]) --> Awake[Awake: Refresh]
    Awake --> Refresh[Refresh: 정보 표시]
    
    Refresh --> ShowEmail[이메일 표시]
    Refresh --> ShowTag[태그 표시]
    Refresh --> ShowNickname[닉네임 표시]
    
    ClickPasswordChange[비밀번호 변경 클릭] --> ChangePassword[AccountManager.ChangePassword]
    ChangePassword --> ShowMessage1[UI_MessagePopup: 이메일 전송 완료]
    
    ClickDeleteAccount[계정 삭제 클릭] --> OpenWithdraw[PopupManager.Open: UI_WithdrawPopup]
    
    ClickChangeNickname[닉네임 변경 클릭] --> ShowConfirm[UI_MessagePopup: 변경 확인]
    ShowConfirm --> UserConfirm{사용자 확인?}
    UserConfirm -->|Yes| OnSetNickname[OnSetNickname 실행]
    OnSetNickname --> SetNickname[AccountManager.SetNickname]
    SetNickname --> CheckResult{결과 성공?}
    CheckResult -->|Yes| ShowSuccess[UI_MessagePopup: 성공]
    ShowSuccess --> Refresh
    ShowSuccess --> SubtractCurrency[CurrencyManager.SubtractCurrency]
    CheckResult -->|No| ShowFail[UI_MessagePopup: 실패]
    
    ClickMyInfo[내 정보 클릭] --> ShowNotSupported[UI_MessagePopup: 미지원]
    
    ClickPrivacy[개인정보처리방침 클릭] --> OpenURL1[Application.OpenURL]
    ClickTerms[이용약관 클릭] --> OpenURL2[Application.OpenURL]
    
    style Start fill:#e1f5ff
    style ShowSuccess fill:#c8e6c9
    style ShowFail fill:#ffcdd2
```

## 5. UI_MessagePopup 플로우

```mermaid
flowchart TD
    Start([메시지 팝업]) --> Init[Init 호출]
    Init --> SetText[메시지 텍스트 설정]
    Init --> SetCallback{콜백 있음?}
    SetCallback -->|Yes| SaveCallback[콜백 저장]
    SetCallback -->|No| NoCallback[콜백 없음]
    
    Init --> CheckCancel{취소 버튼 필요?}
    CheckCancel -->|Yes| ShowBothButtons[OK, Cancel 버튼 표시]
    CheckCancel -->|No| ShowOKOnly[OK 버튼만 표시]
    
    ClickOK[OK 버튼 클릭] --> InvokeCallback{콜백 있음?}
    InvokeCallback -->|Yes| ExecuteCallback[콜백 실행]
    InvokeCallback -->|No| NoAction[아무 동작 없음]
    ExecuteCallback --> ClearCallback[콜백 초기화]
    NoAction --> ClearCallback
    ClearCallback --> Close[Close]
    
    ClickCancel[Cancel 버튼 클릭] --> ClearCallback2[콜백 초기화]
    ClearCallback2 --> Close2[Close]
    
    style Start fill:#e1f5ff
    style ExecuteCallback fill:#c8e6c9
    style Close fill:#fff9c4
    style Close2 fill:#fff9c4
```

## 6. UI_WithdrawPopup 플로우

```mermaid
flowchart TD
    Start([회원탈퇴 팝업]) --> ClickClose[닫기 버튼 클릭]
    ClickClose --> Close[Close]
    
    Start --> ClickConfirm[확인 버튼 클릭]
    ClickConfirm --> DeleteAccount[AccountManager.DeleteAccount]
    DeleteAccount --> Logout[AccountManager.Logout]
    Logout --> CheckScene{리다이렉트 씬 있음?}
    CheckScene -->|Yes| LoadScene[SceneManager.LoadScene: Photon]
    CheckScene -->|No| NoLoad[씬 로드 안함]
    LoadScene --> Close2[Close]
    NoLoad --> Close2
    
    style Start fill:#e1f5ff
    style DeleteAccount fill:#ffcdd2
    style LoadScene fill:#fff9c4
```

## 7. UI_PasswordPopup 플로우

```mermaid
flowchart TD
    Start([비밀번호 팝업]) --> SetRoomInfo[SetRoomInfo: 방 정보 설정]
    SetRoomInfo --> SavePassword[비밀번호 저장]
    
    PasswordCheck[비밀번호 확인 클릭] --> CheckEmpty{입력값 비어있음?}
    CheckEmpty -->|Yes| Fail[Fail]
    CheckEmpty -->|No| CheckMatch{비밀번호 일치?}
    
    CheckMatch -->|No| Fail
    CheckMatch -->|Yes| Success[Success]
    
    Fail --> OpenWrongPopup[PopupManager.Open: UI_PasswordWrongPopup]
    
    Success --> Close[Close]
    Close --> JoinRoom[PhotonNetwork.JoinRoom]
    
    style Start fill:#e1f5ff
    style Success fill:#c8e6c9
    style Fail fill:#ffcdd2
    style JoinRoom fill:#c8e6c9
```

## 8. UI_RoomMakerPopup 플로우

```mermaid
flowchart TD
    Start([방 만들기 팝업]) --> StartInit[Start: 기본값 설정]
    StartInit --> OnEnable[OnEnable: UI 초기화]
    OnEnable --> InitPlayTime[PlayTime.Init]
    OnEnable --> InitLife[Life.Init]
    OnEnable --> InitGunpowder[Gunpowder.Init]
    OnEnable --> InitDecline[Decline.Init]
    
    ClickCreateRoom[방 만들기 클릭] --> GetRoomName[방 이름 가져오기]
    GetRoomName --> CheckEmpty{방 이름 비어있음?}
    CheckEmpty -->|Yes| UseDefaultName[기본 이름 사용]
    CheckEmpty -->|No| CheckLength{길이 3자 이상?}
    
    CheckLength -->|No| ShowError[UI_MessagePopup: 3자 이상 필요]
    CheckLength -->|Yes| CheckLocked{잠금 방?}
    
    CheckLocked -->|No| ClearPassword[비밀번호 초기화]
    CheckLocked -->|Yes| KeepPassword[비밀번호 유지]
    
    UseDefaultName --> CheckLocked
    ClearPassword --> MakeRoom[LobbyManager.MakeRoom]
    KeepPassword --> MakeRoom
    MakeRoom --> Close[Close]
    
    OnDisable[OnDisable] --> ResetUI[UI 초기화]
    ResetUI --> ResetLocked[잠금 해제]
    ResetUI --> ResetRoomName[방 이름 초기화]
    ResetUI --> ResetPassword[비밀번호 초기화]
    ResetUI --> ResetMaxPlayer[최대 인원 초기화]
    
    ClickMaxPlayer[최대 인원 클릭] --> UpdateMaxPlayer[MaxPlayerCount 업데이트]
    
    LockedButton[잠금 버튼] --> TogglePasswordField[비밀번호 필드 활성화/비활성화]
    
    style Start fill:#e1f5ff
    style MakeRoom fill:#c8e6c9
    style ShowError fill:#ffcdd2
    style Close fill:#fff9c4
```

## 9. UI_RoomSearchPopup 플로우

```mermaid
flowchart TD
    Start([방 검색 팝업]) --> OnEnable[OnEnable]
    OnEnable --> SubscribeEvent[EventManager.OnRoomListUpdate 구독]
    SubscribeEvent --> Refresh[Refresh]
    Refresh --> PageSetting[PageSetting]
    
    Refresh --> GetRoomList[방 리스트 가져오기]
    GetRoomList --> CheckEmpty{방 리스트 비어있음?}
    CheckEmpty -->|Yes| HideAllSlots[모든 슬롯 숨기기]
    CheckEmpty -->|No| CalculatePage[페이지 수 계산]
    
    CalculatePage --> CalculateStartIndex[시작 인덱스 계산]
    CalculateStartIndex --> CheckPage{현재 페이지 유효?}
    CheckPage -->|No| AdjustPage[페이지 조정]
    CheckPage -->|Yes| DisplayRooms[방 표시]
    
    AdjustPage --> DisplayRooms
    DisplayRooms --> LoopSlots[슬롯 반복]
    LoopSlots --> GetMapIcon[맵 아이콘 가져오기]
    GetMapIcon --> RefreshSlot[슬롯 새로고침]
    RefreshSlot --> ShowSlot[슬롯 표시]
    
    PageSetting --> UpdatePageText[페이지 텍스트 업데이트]
    
    ClickSetPage[페이지 변경 클릭] --> ClampPage[페이지 범위 제한]
    ClampPage --> Refresh
    
    OnDisable[OnDisable] --> ResetPage[페이지 초기화]
    ResetPage --> UnsubscribeEvent[이벤트 구독 해제]
    
    style Start fill:#e1f5ff
    style DisplayRooms fill:#c8e6c9
    style HideAllSlots fill:#fff9c4
```

## 10. UI_RoomSetupButton 플로우

```mermaid
flowchart TD
    Start([방 설정 버튼]) --> Init[Init 호출]
    Init --> SetCurrentValue[현재 값 초기화]
    SetCurrentValue --> Refresh[Refresh: 텍스트 업데이트]
    
    ValueUpDown[값 증가/감소] --> ClampValue[값 범위 제한]
    ClampValue --> Refresh
    
    Reset[Reset 호출] --> SetValue[값 설정]
    SetValue --> UpdateText[텍스트 업데이트]
    
    CurrentValue[CurrentValue 호출] --> ReturnValue[현재 값 반환]
    
    OnDisable[OnDisable] --> ResetToInit[초기값으로 리셋]
    
    style Start fill:#e1f5ff
    style Refresh fill:#c8e6c9
    style ClampValue fill:#fff9c4
```

## 11. 전체 시스템 통합 플로우

```mermaid
flowchart TD
    Start([게임 시작]) --> PhotonInit[PhotonServerManager 초기화]
    PhotonInit --> ConnectPhoton[Photon 서버 연결]
    ConnectPhoton --> LoadLobby[로비 씬 로드]
    
    LoadLobby --> PopupManagerInit[PopupManager 초기화]
    PopupManagerInit --> UserAction{사용자 액션}
    
    UserAction -->|방 만들기| OpenRoomMaker[UI_RoomMakerPopup 열기]
    UserAction -->|방 검색| OpenRoomSearch[UI_RoomSearchPopup 열기]
    UserAction -->|설정| OpenSetting[UI_SettingPopup 열기]
    UserAction -->|정보| OpenInformation[UI_InformationPopup 열기]
    
    OpenRoomMaker --> CreateRoom[방 생성]
    CreateRoom --> JoinRoom[방 입장]
    
    OpenRoomSearch --> SelectRoom{방 선택}
    SelectRoom -->|비밀번호 있음| OpenPassword[UI_PasswordPopup 열기]
    SelectRoom -->|비밀번호 없음| JoinRoom
    
    OpenPassword --> CheckPassword{비밀번호 확인}
    CheckPassword -->|성공| JoinRoom
    CheckPassword -->|실패| OpenWrong[UI_PasswordWrongPopup 열기]
    
    JoinRoom --> WaitingRoom[대기실]
    WaitingRoom --> GameStart{게임 시작}
    GameStart --> GameScene[게임 씬]
    
    GameScene --> EscapeKey{Escape 키}
    EscapeKey --> OpenMenu[UI_MenuPopup 열기]
    OpenMenu --> MenuAction{메뉴 액션}
    MenuAction -->|설정| OpenSetting
    MenuAction -->|정보| OpenInformation
    MenuAction -->|나가기| LeaveRoom[방 나가기]
    
    OpenInformation --> InfoAction{정보 액션}
    InfoAction -->|비밀번호 변경| ChangePassword[비밀번호 변경]
    InfoAction -->|닉네임 변경| ChangeNickname[닉네임 변경]
    InfoAction -->|계정 삭제| OpenWithdraw[UI_WithdrawPopup 열기]
    
    OpenWithdraw --> ConfirmDelete{삭제 확인}
    ConfirmDelete -->|확인| DeleteAccount[계정 삭제]
    DeleteAccount --> Logout[로그아웃]
    Logout --> PhotonScene[Photon 씬]
    
    style Start fill:#e1f5ff
    style LoadLobby fill:#c8e6c9
    style GameScene fill:#c8e6c9
    style DeleteAccount fill:#ffcdd2
    style PhotonScene fill:#fff9c4
```

