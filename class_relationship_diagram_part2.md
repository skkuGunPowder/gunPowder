# 클래스 관계도 및 의존성 분석 (Part 2)

## 1. 전체 클래스 관계도

```mermaid
classDiagram
    class MonoBehaviour {
        <<Unity>>
    }
    
    class MonoBehaviourPunCallbacks {
        <<Photon>>
    }
    
    class PhotonSingleton~T~ {
        <<abstract>>
        +static T Instance
        #Awake()
    }
    
    class Singleton~T~ {
        <<abstract>>
        +static T Instance
        #Awake()
    }
    
    class UI_Popup {
        <<abstract>>
        +Open(Action)
        +Close()
    }
    
    class CameraController {
        -Camera _mainCamera
        -ProCamera2D _proCamera
        -Player _target
        -bool _isObserving
        -List~Player~ _currentTargetList
        -int _currentTargetIndex
        +float TargetZoomDuration
        +float TargetZoomAmount
        +event Action~bool~ OnUIOnOff
        +event Action~string~ OnNicknameChanged
        +SetTarget(Player)
        +ExplosionShake(Transform, float)
        +SmallShakeAt(Transform, float)
        +SelectTarget(int)
        -HitShake()
        -GunShotShake()
        -SetObserveTarget()
        -PlayObservingMode()
        -LastAttack(int)
    }
    
    class UI_RoomTapManager {
        +List~UI_TapSlot~ TapDataList
        +SelectTap(ETapOption)
        +Onclick(ETapOption)
        +OnclickOnlyMaster(ETapOption)
        -Init()
    }
    
    class UI_TapSlot {
        +ETapOption TapType
        +GameObject TapObject
        +Button TapButton
        +bool IsMaster
        +SetActive(ETapOption)
    }
    
    class UI_RoomStartOption {
        +Button MapSelectButton
        +Image MapIcon
        +TextMeshProUGUI MapNameGUGI
        +ButtonSetup()
        +MapChange(EMap)
        -Refresh(string, Sprite)
    }
    
    class UI_RoomSetting {
        -int PlaytimeInit
        -int PowderInit
        -int DeclineInit
        -int LifeInit
        +UI_RoomSetupButton Playtime
        +UI_RoomSetupButton Powder
        +UI_RoomSetupButton Life
        +AcceptButton()
        +CancelButton()
        -Init(ERoomProperties, UI_RoomSetupButton) int
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
    
    class UI_ToLobbyButton {
        -float _clickDelay
        -float _timer
        -bool _isClick
        -Button _button
        +OnClickToLobby()
    }
    
    class UI_RoomProfile {
        +List~UI_ProfileSlot~ UI_ProfileSlotList
        +Refresh()
        +ReadyCheck(PhotonPlayer)
        +TeamChange()
    }
    
    class UI_ProfileSlot {
        +Refresh(PhotonPlayer)
        +MasterCheck(bool)
        +ReadyCheck(PhotonPlayer)
        +TeamSet(PhotonPlayer)
    }
    
    class UI_MapSelectPopup {
        +List~UI_MapSelectButton~ UI_MapSelectButtonList
        +List~UI_ThemeButton~ UI_ThemeButtonList
        +EMap Map
        +EMapTheme Theme
        -List~MapData~ _mapDataList
        +ChangeTheme(EMapTheme)
        -Init()
        -LoadCurrentMap(EMap, EMapTheme)
        -Refresh(EMapTheme)
        -ThemeRefresh(EMapTheme)
        -MapRefresh(List~MapData~)
        -SetMapList(List~MapData~)
    }
    
    class UI_MapSelectButton {
        +Refresh(EMap, bool, Sprite, string)
    }
    
    class UI_ThemeButton {
        +UI_MapSelectPopup UI_MapSelectPopup
        +EMapTheme Theme
        +TextMeshProUGUI ThemeNameText
        +Image ThemeImage
        +GameObject IsSelected
        +Refresh(EMapTheme, Sprite, string)
        +SelectCheck(EMapTheme)
        +OnClickTheme()
    }
    
    class RoomManager {
        -Room _room
        -PhotonView _photonView
        +int MaxPlayerCount
        +RoomInitializer Initializer
        +RoomReadyCheck ReadyCheck
        +RoomPlayerList PlayerList
        +PlayerSpawner Spawner
        +EMap SelectedMap
        +EInGameTeam SelectedTeam
        -bool _initialized
        +GameStart()
        +PlayerLeft(PhotonPlayer)
        +Rpc_UpdateSlots(int[])
        -Init()
        -SetRoom()
    }
    
    class RoomInitializer {
        +Init(RoomManager)
        +PlayerInitial(int)
    }
    
    class RoomReadyCheck {
        +CheckReady() bool
    }
    
    class RoomPlayerList {
        +List~int~ PlayerSlotList
        +AddPlayerPlacement(PhotonPlayer)
        +SubPlayerPlacement(PhotonPlayer)
        +GetPlayerList(int[])
        +PlayerListCheck()
    }
    
    class GameManager {
        -Dictionary~EInGameTeam,int~ _teamCount
        -PhotonView _photonView
        -EGameState _currentGameState
        +bool LastPlayer
        -GameObject _myPlayer
        +Transform ResurrectPoint
        +event Action~PhotonPlayer~ OnTimeCheck
        +RequestGameOver()
        +GameStartSetting()
        +GameResultCheck()
        +GameStateChange(EGameState)
        +TimeScaleSetting()
        -Init()
        -PlayerLastCheck(PhotonPlayer)
        -PlayerDeadCheck()
        -GameOverToPlayerLeft()
        -TeamSetting()
        -LastTeamCheck() int
        -LastAttackCheck(EInGameTeam) bool
        +RPC_GameStart()
        +RPC_GameOver()
        +RPC_LastPlayer()
        +RPC_RequestPlayerDie(bool)
    }
    
    class EmotionManager {
        +PhotonView MyPhotonView
        -List~int~ _myEmotionList
        -Dictionary~KeyCode,int~ _emotionKeyDictionary
        -List~PlayerEmotion~ _playerEmotionList
        +float EmotionCoolTime
        -float _timer
        -bool _canEmotion
        -Request_PlayerFind()
        -Request_PlayEmotion(int)
        -CoolDown()
        -DefaultEmotion()
        +RPC_PlayerFind()
        +RPC_PlayEmotion(int, PhotonMessageInfo)
    }
    
    class PlayerEmotion {
        +Play(string)
        +GetPlayerNumber() int
        +IsLive bool
    }
    
    class PlayerSpawner {
        +GameObject PlayerPrefab
        +List~Transform~ SpawnPoints
        +List~RankSpawnPoint~ RankSpawnPointList
        +GeneratePlayers(int)
        +GeneratePlayers(int, int)
    }
    
    class RoomStatManager {
        +int PlayerLife
        +int PlayerGunpowder
        +int PlayerDecreaseTime
        +EInGameTeam PlayerTeam
        +bool IsManual
    }
    
    class SceneComplete {
        -float _duration
    }
    
    class EventManager {
        <<Singleton>>
        +event Action~int~ OnTopPlayerChanged
        +event Action~int,int,int,int~ OnDataChanged
        +event Action~EMap~ OnMapChanged
        +event Action OnRoomDataChanged
        +event Action~PhotonPlayer~ OnReadyChanged
        +event Action OnMasterChanged
        +event Action~PhotonPlayer~ OnPlayerLeft
        +event Action OnTeamChanged
        +event Action OnTargetChanged
        +event Action~int~ OnLastAttack
        +event Action OnPlayObserve
        +event Action OnLoadFinished
        +event Action OnGameStart
        +event Action OnGameOver
    }
    
    class MapDataManager {
        <<Singleton>>
        +GetDataLoad(EMap) MapData
        +GetThemeData(EMap) MapThemeData
        +GetThemeDataList() List~MapThemeData~
        +ThemeDataLoad(EMapTheme) List~MapData~
        +LoadMapData()
        +event Action~EMap,EMapTheme~ OnMapDataLoad
    }
    
    class TransitionManager {
        <<Singleton>>
        +LoadLevel(ESceneList)
        +EndAnimation(float)
    }
    
    class UltimateManager {
        <<Singleton>>
        +SetPlayer(Player)
    }
    
    class Player {
        +PhotonView PhotonView
        +event Action OnHit
        +event Action OnAttack
    }
    
    %% 상속 관계
    MonoBehaviour <|-- CameraController : extends
    MonoBehaviour <|-- UI_RoomTapManager : extends
    MonoBehaviour <|-- UI_RoomStartOption : extends
    MonoBehaviour <|-- UI_RoomSetting : extends
    MonoBehaviour <|-- UI_RoomSetupButton : extends
    MonoBehaviour <|-- UI_ToLobbyButton : extends
    MonoBehaviour <|-- UI_RoomProfile : extends
    MonoBehaviour <|-- UI_TapSlot : extends
    MonoBehaviour <|-- UI_ProfileSlot : extends
    MonoBehaviour <|-- UI_MapSelectButton : extends
    MonoBehaviour <|-- UI_ThemeButton : extends
    MonoBehaviour <|-- EmotionManager : extends
    MonoBehaviour <|-- PlayerSpawner : extends
    MonoBehaviour <|-- SceneComplete : extends
    MonoBehaviour <|-- Player : extends
    
    PhotonSingleton~T~ <|-- RoomManager : extends
    PhotonSingleton~T~ <|-- GameManager : extends
    MonoBehaviourPunCallbacks <|-- PhotonSingleton~T~ : extends
    
    Singleton~T~ <|-- RoomStatManager : extends
    MonoBehaviour <|-- Singleton~T~ : extends
    
    UI_Popup <|-- UI_MapSelectPopup : extends
    MonoBehaviour <|-- UI_Popup : extends
    
    %% 구성 관계
    UI_RoomTapManager *-- UI_TapSlot : contains List
    UI_RoomProfile *-- UI_ProfileSlot : contains List
    UI_MapSelectPopup *-- UI_MapSelectButton : contains List
    UI_MapSelectPopup *-- UI_ThemeButton : contains List
    UI_RoomSetting *-- UI_RoomSetupButton : contains 3x
    RoomManager *-- RoomInitializer : contains
    RoomManager *-- RoomReadyCheck : contains
    RoomManager *-- RoomPlayerList : contains
    RoomManager *-- PlayerSpawner : contains
    
    %% 의존성 관계
    CameraController --> EventManager : subscribes to
    CameraController --> Player : uses
    
    UI_RoomStartOption --> EventManager : subscribes to
    UI_RoomStartOption --> MapDataManager : uses Instance
    
    UI_RoomSetting --> PhotonNetwork : uses
    UI_RoomSetting --> UI_RoomSetupButton : uses
    
    UI_ToLobbyButton --> PhotonNetwork : uses
    
    UI_RoomProfile --> EventManager : subscribes to
    UI_RoomProfile --> RoomManager : uses Instance
    UI_RoomProfile --> PhotonNetwork : uses
    
    UI_MapSelectPopup --> MapDataManager : uses Instance
    
    UI_ThemeButton --> UI_MapSelectPopup : uses
    
    RoomManager --> EventManager : uses Instance
    RoomManager --> PhotonNetwork : uses
    RoomManager --> UIChatManager : uses Instance
    
    GameManager --> EventManager : subscribes to
    GameManager --> PhotonNetwork : uses
    GameManager --> Player : uses
    GameManager --> PlayerFSM : uses
    GameManager --> PlayerStat : uses
    
    EmotionManager --> EventManager : subscribes to
    EmotionManager --> PhotonNetwork : uses
    EmotionManager --> PlayerEmotion : uses List
    
    PlayerSpawner --> PhotonNetwork : uses
    PlayerSpawner --> CameraController : uses
    PlayerSpawner --> UltimateManager : uses Instance
    PlayerSpawner --> Player : creates
    
    RoomStatManager --> PhotonNetwork : uses
    
    SceneComplete --> TransitionManager : uses Instance
```

## 2. RoomManager 구조 상세

```mermaid
graph TB
    subgraph "RoomManager 시스템"
        RM[RoomManager<br/>PhotonSingleton]
        
        subgraph "내부 컴포넌트"
            RI[RoomInitializer<br/>방 초기화]
            RC[RoomReadyCheck<br/>준비 상태 체크]
            RPL[RoomPlayerList<br/>플레이어 리스트 관리]
            PS[PlayerSpawner<br/>플레이어 스폰]
        end
        
        subgraph "외부 의존성"
            EM[EventManager]
            PNW[PhotonNetwork]
            UCM[UIChatManager]
        end
        
        subgraph "UI 컴포넌트"
            URP[UI_RoomProfile]
            URS[UI_RoomSetting]
            URSO[UI_RoomStartOption]
            URTM[UI_RoomTapManager]
            UTLB[UI_ToLobbyButton]
        end
    end
    
    RM -->|포함| RI
    RM -->|포함| RC
    RM -->|포함| RPL
    RM -->|포함| PS
    
    RM -->|사용| EM
    RM -->|사용| PNW
    RM -->|사용| UCM
    
    URP -->|구독| EM
    URP -->|사용| RM
    URS -->|사용| PNW
    URSO -->|구독| EM
    URSO -->|사용| MapDataManager
    
    style RM fill:#4CAF50
    style RI fill:#2196F3
    style RC fill:#2196F3
    style RPL fill:#2196F3
    style PS fill:#2196F3
```

## 3. GameManager 의존성 상세

```mermaid
graph TD
    subgraph "GameManager 시스템"
        GM[GameManager<br/>PhotonSingleton]
        
        subgraph "이벤트 구독"
            EM[EventManager]
            EM -->|OnLoadFinished| GM
            EM -->|OnPlayerLeft| GM
        end
        
        subgraph "게임 상태 관리"
            GS[EGameState]
            TS[TimeScale]
        end
        
        subgraph "플레이어 관리"
            PC[Player 체크]
            TC[Team 체크]
            LC[LastPlayer 체크]
        end
        
        subgraph "RPC 메서드"
            RPC1[RPC_GameStart]
            RPC2[RPC_GameOver]
            RPC3[RPC_LastPlayer]
            RPC4[RPC_RequestPlayerDie]
        end
        
        subgraph "플레이어 컴포넌트"
            P[Player]
            PFSM[PlayerFSM]
            PS[PlayerStat]
        end
    end
    
    GM -->|구독| EM
    GM -->|사용| PNW[PhotonNetwork]
    GM -->|사용| P
    GM -->|사용| PFSM
    GM -->|사용| PS
    
    GM -->|발행| RPC1
    GM -->|발행| RPC2
    GM -->|발행| RPC3
    GM -->|발행| RPC4
    
    style GM fill:#4CAF50
    style EM fill:#FF9800
    style P fill:#2196F3
```

## 4. CameraController 의존성

```mermaid
graph LR
    subgraph "CameraController 시스템"
        CC[CameraController]
        
        subgraph "카메라 컴포넌트"
            MC[Camera _mainCamera]
            PC[ProCamera2D _proCamera]
        end
        
        subgraph "타겟 관리"
            T[Player _target]
            TL[List~Player~ _currentTargetList]
        end
        
        subgraph "이벤트"
            EM[EventManager]
            CC -->|구독| EM
            EM -->|OnTargetChanged| CC
            EM -->|OnLastAttack| CC
            EM -->|OnPlayObserve| CC
        end
        
        subgraph "이벤트 발행"
            UIO[OnUIOnOff]
            NC[OnNicknameChanged]
        end
        
        subgraph "쉐이크 효과"
            PS[ProCamera2DShake]
        end
    end
    
    CC -->|사용| MC
    CC -->|사용| PC
    CC -->|관리| T
    CC -->|관리| TL
    CC -->|사용| PS
    
    CC -->|발행| UIO
    CC -->|발행| NC
    
    style CC fill:#4CAF50
    style EM fill:#FF9800
    style PS fill:#9C27B0
```

## 5. UI 컴포넌트 관계도

```mermaid
graph TB
    subgraph "대기실 UI 시스템"
        subgraph "탭 관리"
            URTM[UI_RoomTapManager]
            UTS[UI_TapSlot]
            URTM -->|관리| UTS
        end
        
        subgraph "프로필 관리"
            URP[UI_RoomProfile]
            UPS[UI_ProfileSlot]
            URP -->|관리| UPS
        end
        
        subgraph "방 설정"
            URS[UI_RoomSetting]
            URSB[UI_RoomSetupButton]
            URS -->|사용| URSB
        end
        
        subgraph "맵 선택"
            URSO[UI_RoomStartOption]
            UMSP[UI_MapSelectPopup]
            UMSB[UI_MapSelectButton]
            UTB[UI_ThemeButton]
            UMSP -->|관리| UMSB
            UMSP -->|관리| UTB
            UTB -->|호출| UMSP
        end
        
        subgraph "기타"
            UTLB[UI_ToLobbyButton]
        end
    end
    
    subgraph "외부 매니저"
        RM[RoomManager]
        EM[EventManager]
        MDM[MapDataManager]
    end
    
    URP -->|구독| EM
    URP -->|사용| RM
    URSO -->|구독| EM
    URSO -->|사용| MDM
    URS -->|사용| PhotonNetwork
    UTLB -->|사용| PhotonNetwork
    
    style URTM fill:#4CAF50
    style URP fill:#2196F3
    style URS fill:#FF9800
    style UMSP fill:#9C27B0
```

## 6. EmotionManager 시스템

```mermaid
graph TD
    subgraph "EmotionManager 시스템"
        EM[EmotionManager]
        
        subgraph "이벤트 구독"
            EVM[EventManager]
            EVM -->|OnPlayerFind| EM
        end
        
        subgraph "이모션 관리"
            MEL[List~int~ _myEmotionList]
            EKD[Dictionary~KeyCode,int~ _emotionKeyDictionary]
            PEL[List~PlayerEmotion~ _playerEmotionList]
        end
        
        subgraph "RPC 통신"
            RPC1[RPC_PlayerFind]
            RPC2[RPC_PlayEmotion]
        end
        
        subgraph "쿨다운"
            CT[EmotionCoolTime]
            T[_timer]
            CE[_canEmotion]
        end
    end
    
    EM -->|사용| PV[PhotonView]
    EM -->|사용| PNW[PhotonNetwork]
    EM -->|관리| MEL
    EM -->|관리| EKD
    EM -->|관리| PEL
    
    EM -->|발행| RPC1
    EM -->|발행| RPC2
    
    style EM fill:#4CAF50
    style EVM fill:#FF9800
    style PV fill:#2196F3
```

## 7. PlayerSpawner 시스템

```mermaid
graph TD
    subgraph "PlayerSpawner 시스템"
        PS[PlayerSpawner]
        
        subgraph "스폰 포인트"
            SP[List~Transform~ SpawnPoints]
            RSP[List~RankSpawnPoint~ RankSpawnPointList]
        end
        
        subgraph "생성"
            PP[GameObject PlayerPrefab]
            P[Player]
        end
        
        subgraph "의존성"
            PNW[PhotonNetwork]
            CC[CameraController]
            UM[UltimateManager]
        end
    end
    
    PS -->|포함| SP
    PS -->|포함| RSP
    PS -->|사용| PP
    PS -->|생성| P
    PS -->|사용| PNW
    PS -->|사용| CC
    PS -->|사용| UM
    
    P -->|설정| CC
    P -->|설정| UM
    
    style PS fill:#4CAF50
    style P fill:#2196F3
    style CC fill:#FF9800
```

## 8. 전체 시스템 통합 관계도

```mermaid
graph TB
    subgraph "게임 시작 흐름"
        SC[SceneComplete]
        SC -->|EndAnimation| TM[TransitionManager]
    end
    
    subgraph "대기실 시스템"
        RM[RoomManager]
        RSM[RoomStatManager]
        URP[UI_RoomProfile]
        URS[UI_RoomSetting]
        URSO[UI_RoomStartOption]
        URTM[UI_RoomTapManager]
        
        RM -->|초기화| RSM
        RM -->|데이터 제공| URP
        URP -->|구독| EM[EventManager]
        URS -->|설정 변경| RM
        URSO -->|맵 선택| RM
    end
    
    subgraph "게임 시작"
        RM -->|GameStart| PNW[PhotonNetwork.LoadLevel]
        PS[PlayerSpawner] -->|생성| P[Player]
        PS -->|설정| CC[CameraController]
        PS -->|설정| UM[UltimateManager]
    end
    
    subgraph "게임 중"
        GM[GameManager]
        CC -->|관찰 모드| P
        EM2[EmotionManager] -->|이모션| PE[PlayerEmotion]
        GM -->|게임 상태 관리| EM
    end
    
    subgraph "이벤트 허브"
        EM -->|발행| 모든 구독자
    end
    
    style RM fill:#4CAF50
    style GM fill:#4CAF50
    style EM fill:#FF9800
    style CC fill:#2196F3
```

## 9. 이벤트 구독 관계 상세

```mermaid
graph LR
    subgraph "EventManager 이벤트 발행"
        EM[EventManager]
    end
    
    subgraph "이벤트 구독자"
        CC[CameraController]
        URSO[UI_RoomStartOption]
        URP[UI_RoomProfile]
        GM[GameManager]
        EM2[EmotionManager]
    end
    
    subgraph "이벤트 타입"
        E1[OnTargetChanged]
        E2[OnLastAttack]
        E3[OnPlayObserve]
        E4[OnMapChanged]
        E5[OnMasterChanged]
        E6[OnRoomDataChanged]
        E7[OnReadyChanged]
        E8[OnTeamChanged]
        E9[OnLoadFinished]
        E10[OnPlayerLeft]
        E11[OnPlayerFind]
    end
    
    EM -->|발행| E1
    EM -->|발행| E2
    EM -->|발행| E3
    EM -->|발행| E4
    EM -->|발행| E5
    EM -->|발행| E6
    EM -->|발행| E7
    EM -->|발행| E8
    EM -->|발행| E9
    EM -->|발행| E10
    EM -->|발행| E11
    
    E1 -->|구독| CC
    E2 -->|구독| CC
    E3 -->|구독| CC
    E4 -->|구독| URSO
    E5 -->|구독| URSO
    E6 -->|구독| URP
    E7 -->|구독| URP
    E8 -->|구독| URP
    E9 -->|구독| GM
    E10 -->|구독| GM
    E11 -->|구독| EM2
    
    style EM fill:#FF9800
    style CC fill:#4CAF50
    style URSO fill:#2196F3
    style URP fill:#2196F3
    style GM fill:#4CAF50
    style EM2 fill:#9C27B0
```

## 10. 데이터 흐름 시퀀스 다이어그램

```mermaid
sequenceDiagram
    participant User as 사용자
    participant URS as UI_RoomSetting
    participant RM as RoomManager
    participant PNW as PhotonNetwork
    participant EM as EventManager
    participant URP as UI_RoomProfile
    
    User->>URS: 설정 변경
    URS->>PNW: SetCustomProperties
    PNW->>RM: OnRoomPropertiesUpdate
    RM->>EM: RoomDataChanged()
    EM->>URP: OnRoomDataChanged 이벤트
    URP->>URP: Refresh()
    
    Note over User,URP: 방 설정 변경 흐름
    
    participant URSO as UI_RoomStartOption
    participant MDM as MapDataManager
    
    User->>URSO: 맵 선택 버튼 클릭
    URSO->>MDM: GetDataLoad()
    MDM-->>URSO: MapData
    URSO->>URSO: Refresh()
    
    Note over User,URSO: 맵 선택 흐름
    
    participant GM as GameManager
    participant PS as PlayerSpawner
    participant CC as CameraController
    
    RM->>PNW: LoadLevel()
    PNW->>GM: 씬 로드 완료
    GM->>EM: OnLoadFinished 이벤트
    GM->>PS: GeneratePlayers()
    PS->>PNW: Instantiate(Player)
    PS->>CC: SetTarget(Player)
    CC->>CC: 카메라 타겟 설정
```

## 주요 관계 요약

### 상속 관계
- **RoomManager, GameManager** → `PhotonSingleton<T>` → `MonoBehaviourPunCallbacks`
- **RoomStatManager** → `Singleton<T>` → `MonoBehaviour`
- **UI_MapSelectPopup** → `UI_Popup` → `MonoBehaviour`
- 나머지 UI 클래스들 → `MonoBehaviour`

### 구성 관계
- **RoomManager**는 `RoomInitializer`, `RoomReadyCheck`, `RoomPlayerList`, `PlayerSpawner`를 포함
- **UI_RoomTapManager**는 `UI_TapSlot` 리스트를 관리
- **UI_RoomProfile**는 `UI_ProfileSlot` 리스트를 관리
- **UI_MapSelectPopup**는 `UI_MapSelectButton`, `UI_ThemeButton` 리스트를 관리
- **UI_RoomSetting**는 3개의 `UI_RoomSetupButton`을 포함

### 의존성 관계
- **CameraController** → EventManager (구독), Player (사용)
- **UI_RoomStartOption** → EventManager (구독), MapDataManager (사용)
- **UI_RoomProfile** → EventManager (구독), RoomManager (사용)
- **RoomManager** → EventManager (사용), PhotonNetwork (사용)
- **GameManager** → EventManager (구독), PhotonNetwork (사용), Player (사용)
- **EmotionManager** → EventManager (구독), PhotonNetwork (사용), PlayerEmotion (사용)
- **PlayerSpawner** → PhotonNetwork (사용), CameraController (사용), UltimateManager (사용)

### 이벤트 관계
- **EventManager**는 중앙 이벤트 허브로 여러 클래스가 구독/발행
- 주요 이벤트: OnMapChanged, OnRoomDataChanged, OnReadyChanged, OnMasterChanged, OnLoadFinished, OnPlayerLeft, OnTargetChanged, OnLastAttack, OnPlayObserve 등


