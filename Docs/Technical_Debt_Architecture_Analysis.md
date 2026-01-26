# GunPowder 프로젝트 기술 부채 및 아키텍처 분석 보고서

> **분석 일자**: 2026년 1월 26일
> **분석 대상**: `Assets/02.Scripts` 폴더 (371개 C# 스크립트, 총 42,409 라인)
> **분석 목적**: 대규모 리팩토링을 위한 구조적 문제 식별 및 개선 방향 제시

---

## 📊 개요

GunPowder 프로젝트는 Unity + Photon PUN2 + TheBackend SDK 기반의 멀티플레이어 배틀 게임입니다. 현재 코드베이스는 371개의 스크립트로 구성되어 있으나, 여러 아키텍처적 문제점이 존재합니다.

**주요 통계:**
- **총 라인 수**: 42,409 라인
- **가장 큰 파일**: Player.cs (1,552 라인), UIChatManager.cs (1,168 라인)
- **싱글톤 클래스**: 29개 (전역 의존성 위험성)
- **Photon 네트워크 호출**: 260건
- **RPC 사용**: 172건
- **코루틴 사용**: 100건
- **EventManager 직접 호출**: 158건
- **CustomProperties 사용**: 134건 (높은 결합도)

---

## 1. 시스템 구조 다이어그램 (Mermaid)

### 1.1 전체 아키텍처 관계도

```mermaid
classDiagram
    class DontDestroySingleton~T~ {
        <<Abstract>>
        static T Instance
        void Awake()
        void OnDestroy()
    }

    class PhotonSingleton~T~ {
        <<Abstract>>
        static T Instance
        void Awake()
        void OnDestroy()
        OnConnectedToMaster()
        OnJoinedRoom()
        OnPlayerPropertiesUpdate()
    }

    class EventManager {
        <<DontDestroySingleton>>
        -Dictionary~string, Delegate~ events
        +OnPlayerItemChanged
        +OnDataChanged
        +OnGameOver
        +OnGameStart
    }

    class GameManager {
        <<PhotonSingleton>>
        EGameState CurrentGameState
        +CheckGameStatus()
        +SetPlayerDead()
        +SetPlayerAlive()
    }

    class Player {
        <<God Class>>
        PhotonView photonView
        PlayerStat playerStat
        PlayerFSM playerFSM
        PlayerCombat combat
        PlayerGunpowderManager gunpowderManager
        PlayerUltimate ultimate
        PlayerVisuals visuals
        +TakeDamage()
        +RPC_ChangeState()
        +LoadItems()
    }

    class PlayerStat {
        int currentGunPowder
        int currentLife
        EInGameTeam team
        +DecreaseGunPowderCount()
    }

    class RoomManager {
        <<PhotonSingleton>>
        RoomInitializer initializer
        RoomPlayerList playerList
        PlayerSpawner spawner
        +OnPlayerPropertiesUpdate()
        +OnRoomPropertiesUpdate()
    }

    class PhotonServerManager {
        <<PhotonSingleton>>
        +OnConnectedToMaster()
        +OnDisconnected()
        +OnJoinedLobby()
    }

    class AccountManager {
        <<DontDestroySingleton>>
        AccountRepository repository
        +TryLogin()
        +TryRegister()
        +DeleteAccount()
    }

    class UIChatManager {
        <<DontDestroySingleton>>
        ChatClient chatClient
        Dictionary~string, ChannelInfo~ channels
        +SendChatMessage()
        +OnMessage()
        +CoRetryJoinInGameChannel()
    }

    class BackendManager {
        <<DontDestroySingleton>>
        +Initialize()
        +Login()
    }

    class ItemStorage {
        <<DontDestroySingleton>>
        +GetItem()
        +EquipItem()
    }

    %% Relationship lines
    EventManager --|> DontDestroySingleton
    GameManager --|> PhotonSingleton
    RoomManager --|> PhotonSingleton
    PhotonServerManager --|> PhotonSingleton

    AccountManager --> EventManager
    AccountManager --> AccountRepository

    Player --> PlayerStat
    Player --> PlayerFSM
    Player --> EventManager
    Player --> ItemStorage

    RoomManager --> EventManager
    RoomManager --> Player

    UIChatManager --> BackendManager
    UIChatManager --> PhotonServerManager
    Player --> EventManager
    Player --> ItemStorage
    Player --> GameManager
    RoomManager --> EventManager
    RoomManager --> Player
    GameManager --> Player

    %% Network coupling issues
    RoomManager --> PhotonServerManager
    UIChatManager --> PhotonServerManager
    Player --> PhotonServerManager

    %% Backend coupling
    AccountManager --> BackendManager
    AccountManager --> AccountRepository

    %% Data dependencies
    ItemStorage --> BackendManager
    Player --> ItemStorage

    %% Events flow
    EventManager -.-> UI_Inventory
    EventManager -.-> GameManager

    style Player fill:#ff9999
    style RoomManager fill:#ff9999
    style UIChatManager fill:#ff9999
    style AccountManager fill:#ffcc99
    ```

> **범례:**
> - 🔴 빨간색: 높은 결합도 (Tight Coupling)
> - 🟠 주황색: 중간 결합도

---

## 2. 리팩토링 우선순위 TOP 5

| 우선순위 | 클래스명 | 문제점 요약 (Code Smell) | 리팩토링 제안 (Solution) | 예상 난이도 |
| :--- | :--- | :--- | :--- | :--- |
| **1** | **Player.cs** | **God Class**: 1,552 라인, SRP 위반 (전투, 이동, 시각, 네트워크, 스탯, 궁극기, 스킨 관리) | **하위 시스템 분리**: PlayerCombat, PlayerMovement, PlayerVisuals, PlayerNetworkBridge, PlayerStateController로 분리하고 Player에서 조합 패턴 적용 | **상** (상태 전환 관리 주의 필요) |
| **2** | **RoomManager.cs** | **Tight Coupling**: Photon 콜백에 게임 로직(씬 로딩, 게임 시작, UI 업데이트)이 혼재 | **계층 분리**: NetworkAdapter (Photon 콜백만 담당) + GameRoomCoordinator (게임 로직) 분리. Observer 패턴으로 상태 변경 통지 | **상** (네트워크 동기화 재설계 필요) |
| **3** | **UIChatManager.cs** | **God Class**: 1,168 라인, 채널 관리 + 백엔드 SDK + UI 로직 혼재 | **MVP 패턴**: ChatBusinessLogic (도메인), ChatViewModel (데이터 바인딩), ChatView (UI 표시)로 분리 | **중** (UI/로직 분리 작업) |
| **4** | **GameManager.cs** | **SRP 위반**: 게임 상태 + 생존 체크 + 종료 조건 + 플레이어 생사 관리 모두 포함 | **상태 패턴 강화**: MatchFlow, GamePhaseManager, SurvivalChecker로 분리하고 GameStateMachine으로 관리 | **중** (상태 머신 재설계) |
| **5** | **싱글톤 스프롤 (29개)** | **Global State**: EventManager, GameManager, AccountManager 등 29개 싱글톤으로 인한 암묵적 의존성 | **Service Locator/DI**: Core Services (EventBus, NetworkBridge, DataStore)만 싱글톤 유지하고 나머지는 의존성 주입으로 대체 | **상** (시스템 전체 영향) |

---

## 3. 주요 모듈별 상세 분석

### 3.1 Core/Manager 모듈

#### 현상
- **GameManager.cs** (423 라인): 게임 상태(Waiting, Playing, GameOver, Result) 관리와 플레이어 생존 체크, 종료 조건 판단을 단일 클래스에서 담당
- **PhotonServerManager.cs**: 포톤 연결/로비 관리와 씬 전환 로직을 콜백 내에서 직접 수행
- **EventManager.cs**: 26개 이상의 게임 이벤트를 중앙 집중식으로 관리하지만, 전역 이벤트 오남용 가능성

#### 문제점
1. **단일 책임 원칙(SRP) 위반**
   - GameManager가 너무 많은 책임을 담당 → 유지보수 어려움
   - 게임 플로우, 플레이어 생존, 종료 로직이 섞여 있어 테스트 어려움

2. **네트워크 콜백과 게임 로직 혼재**
   - PhotonServerManager.OnConnected → LoadLevel(Lobby) 직접 호출
   - 네트워크 레이어가 비즈니스 로직을 직접 수행 → 결합도 높음

3. **전역 상태 관리 위험성**
   - 29개 싱글톤 클래스에서 전역 상태에 접근 → 테스트 어려움
   - 초기화 순서 의존성 (GameManager → RoomManager → Player) 문제

#### 개선안
```csharp
// 1. GameManager 책임 분리
public class MatchFlowCoordinator : MonoBehaviour
{
    // 게임 플로우만 담당 (Waiting → Playing → GameOver → Result)
}

public class SurvivalChecker : MonoBehaviour
{
    // 플레이어 생존 체크만 담당
}

public class GameTerminationDetector : MonoBehaviour
{
    // 게임 종료 조건 판단만 담당
}

// 2. 네트워크 어댑터 패턴
public interface INetworkAdapter
{
    IObservable<Unit> OnConnected { get; }
    IObservable<PlayerData> OnPlayerJoined { get; }
    void Connect();
    void JoinRoom(string roomName);
}

public class PhotonNetworkAdapter : INetworkAdapter
{
    // Photon 콜백 → Observable로 변환
    // 게임 로직은 이 Observable만 구독
}

// 3. 이벤트 시스템 구조화
public interface IGameEventBus
{
    void Publish<T>(T @event) where T : IGameEvent;
    IObservable<T> Subscribe<T>() where T : IGameEvent;
}

// 전역 이벤트 대신 도메인별 이벤트 버스
public class CombatEventBus : IGameEventBus { }
public class UIEventBus : IGameEventBus { }
```

---

### 3.2 Player 모듈

#### 현상
- **Player.cs** (1,552 라인): 모든 플레이어 기능을 하나의 클래스에서 처리
- **PlayerStat.cs** (593 라인): 데이터 중심이지만 플레이어 상태, 궁극기, 통계까지 관리
- **PlayerFSM**: 21개 상태 관리 (Idle, Walk, Run, Jump, Dash, Die 등)
- **하위 컨트롤러**: PlayerCombat, PlayerGunpowderController, PlayerUltimateController, PlayerVisualController, PlayerDamageController, PlayerSkinManager

#### 문제점
1. **God Class (신 클래스)**
   - Player가 전투, 이동, 시각, 네트워크, 스킨, 궁극기 관리를 모두 담당
   - 1,552 라인으로 인한 가독성 저하, 유지보수 어려움
   - 단일 클래스 수정 시 여러 시스템에 영향

2. **하드코딩 및 매직 넘버**
   ```csharp
   // Player.cs 예시
   if (_playerStat.CurrentPlayerLife > 0)  // 매직 넘버
   if (DieSpriteRendererList != null && DieSpriteRendererList.Count > 0)
   ```
   - 매직 넘버가 상수로 정의되지 않음
   - 인스펙터 설정 의존도 높음

3. **네트워크 동기화 복잡도**
   - 172개의 RPC 메서드가 Player 및 하위 클래스에 분산
   - CustomProperties와 로컬 필드 동기화 불일치 가능성 (팀 설정 등)
   - 건파우더 자동 감소 타이머가 각 클라이언트에서 독립 실행 → 동기화 오차

4. **강한 결합(Tight Coupling)**
   - Player가 GameManager, EventManager, ItemStorage, UltimateManager 등 여러 매니저 직접 참조
   - UI 컴포넌트(UI_GunPowderStatus, UI_Ping, UI_Ultimate)가 Player 클래스 직접 참조

#### 개선안
```csharp
// 1. Player 책임 분리 (조합 패턴)
public class Player : MonoBehaviourPun, IDamagable
{
    // 핵심만 유지: 하위 시스템 조합
    public PlayerStat Stat { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerVisuals Visuals { get; private set; }
    public PlayerNetworkBridge Network { get; private set; }

    void Start()
    {
        // 하위 시스템 초기화
        Stat = GetComponent<PlayerStat>();
        Combat = GetComponent<PlayerCombat>();
        Movement = GetComponent<PlayerMovement>();
        Visuals = GetComponent<PlayerVisuals>();
        Network = GetComponent<PlayerNetworkBridge>();
    }

    // IDamagable 구현
    public void TakeDamage(int damage, int attackerViewId)
    {
        Combat.TakeDamage(damage, attackerViewId);
    }
}

// 2. 하위 시스템 분리
public class PlayerCombat : MonoBehaviour
{
    private Player _owner;
    private IBombFactory _bombFactory;

    public void FireBomb(Vector2 direction, EBombType type)
    {
        // 폭탄 발사 로직만 담당
        Bomb bomb = _bombFactory.Create(type, _owner.transform);
        bomb.Launch(direction);
    }

    public void TakeDamage(int damage, int attackerViewId)
    {
        // 피격 처리 로직만 담당
        if (IsAlly(attackerViewId)) return;

        _owner.Stat.DecreaseGunPowder(damage, attackerViewId);
        _owner.Visuals.ShowDamageEffect();
    }
}

public class PlayerMovement : MonoBehaviour
{
    public void Move(Vector2 input, EState state)
    {
        // 이동 로직만 담당
        transform.Translate(input * Time.deltaTime * _moveSpeed);
    }
}

public class PlayerNetworkBridge : MonoBehaviourPun
{
    private Player _owner;

    // 네트워크 동기화만 담당
    [PunRPC]
    public void RPC_TakeDamage(int damage, int attackerViewId)
    {
        if (PhotonView.IsMine)
        {
            _owner.Combat.TakeDamage(damage, attackerViewId);
        }
    }
}

// 3. 상수화 및 설정 분리
public static class PlayerConstants
{
    public const int INITIAL_LIFE = 3;
    public const int INITIAL_GUNPOWDER = 100;
    public const int DAMAGE_PENALTY = 10;
    public const float ATTACK_PENALTY_TIME = 5.0f;
}

// 4. 타이밍 동기화 개선
public class PlayerGunpowderController : MonoBehaviour
{
    private double _lastAttackTime = PhotonNetwork.Time;

    void Update()
    {
        if (!PhotonView.IsMine) return;

        double timeSinceLastAttack = PhotonNetwork.Time - _lastAttackTime;

        if (timeSinceLastAttack >= PlayerConstants.ATTACK_PENALTY_TIME)
        {
            _owner.Stat.DecreaseGunPowder(PlayerConstants.DAMAGE_PENALTY);
            _lastAttackTime = PhotonNetwork.Time;
        }
    }

    public void OnAttack()
    {
        _lastAttackTime = PhotonNetwork.Time;
    }
}
```

---

### 3.3 Network 모듈 (Photon)

#### 현상
- **PhotonServerManager.cs**: 포톤 연결, 로비 입장, 방 목록 갱신 담당
- **RoomManager.cs**: 방 초기화, 플레이어 준비 체크, 맵/팀 동기화 담당
- **LobbyManager.cs**: 방 생성, 방 프로퍼티 설정 담당
- 134번의 CustomProperties 사용 (팀, 준비 상태, 아이템 등)

#### 문제점
1. **콜백과 게임 로직 혼재**
   - RoomManager.OnPlayerPropertiesUpdate → EventManager 호출 + 플레이어 스킨 로드
   - RoomManager.OnRoomPropertiesUpdate → 맵 로드 + 데이터 초기화
   - 네트워크 레이어가 비즈니스 로직을 직접 수행

2. **비동기 처리 예외 누락**
   ```csharp
   // RoomManager.cs 예시
   async UniTaskVoid Delay()
   {
       await UniTask.Delay(2000);  // 2초 대기
       PhotonNetwork.LoadLevel(selectedMap);  // 예외 처리 없음
   }
   ```
   - `async UniTaskVoid` (fire-and-forget) 사용
   - 씬 변경 시 콜백 취소 불가 → 레이스 컨디션 가능

3. **CustomProperties 동기화 복잡도**
   - 팀 변경 시 로컬 필드(`_playerStat.Team`)과 CustomProperties 불일치 가능
   - 속성 변경 감지 콜백에서 즉시 반영되지 않음
   - 순환 참조 가능성 (RoomManager → Player → RoomManager)

#### 개선안
```csharp
// 1. 네트워크 어댑터 + 비즈니스 로직 분리
public interface IRoomNetworkAdapter
{
    IObservable<RoomProperties> OnRoomPropertiesChanged { get; }
    IObservable<PlayerProperties> OnPlayerPropertiesChanged { get; }
    Task CreateRoomAsync(RoomConfig config);
    Task JoinRoomAsync(string roomName);
}

public class PhotonRoomAdapter : IRoomNetworkAdapter
{
    // Photon 콜백 → Observable로 변환만 담당
    // 게임 로직 수행하지 않음
}

public class GameRoomCoordinator : MonoBehaviour
{
    private IRoomNetworkAdapter _network;

    void Start()
    {
        _network.OnRoomPropertiesChanged.Subscribe(OnRoomUpdated);
        _network.OnPlayerPropertiesChanged.Subscribe(OnPlayerUpdated);
    }

    void OnRoomUpdated(RoomProperties props)
    {
        // 비즈니스 로직만 담당
        LoadMap(props.SelectedMap);
        NotifyPlayers(props);
    }
}

// 2. 상태 소유권 명확화
public class PlayerTeamManager
{
    private PhotonView _view;
    private EInGameTeam _cachedTeam;

    public EInGameTeam GetTeam()
    {
        // 항상 CustomProperties에서 읽기
        if (_view.Owner.CustomProperties.TryGetValue("Team", out var team))
        {
            _cachedTeam = (EInGameTeam)team;
            return _cachedTeam;
        }
        return EInGameTeam.Default;
    }

    public void SetTeam(EInGameTeam team)
    {
        if (PhotonView.IsMine)
        {
            var props = new Hashtable { { "Team", team } };
            PhotonView.Owner.SetCustomProperties(props);
        }
    }
}

// 3. 비동기 처리 안전장치
public class NetworkRetryHandler
{
    private CancellationTokenSource _cts;

    public async Task RetryUntilSuccess(Func<Task> operation, int maxRetries, int delayMs)
    {
        _cts = new CancellationTokenSource();

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await operation();
                return; // 성공 시 종료
            }
            catch (Exception ex)
            {
                Debug.LogError($"Retry {i + 1}/{maxRetries} failed: {ex.Message}");

                if (i == maxRetries - 1) throw;

                await Task.Delay(delayMs, _cts.Token);
            }
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }
}
```

---

### 3.4 UI 모듈

#### 현상
- **UI_IngameChatPopup.cs** (819 라인): 채팅 UI 팝업
- **UI_LoginScene.cs** (420 라인): 로그인 씬 UI
- UI 컴포넌트가 Player, GameManager, AccountManager 등 로직 클래스를 직접 참조

#### 문제점
1. **UI와 로직 강한 결합**
   - UI_GunPowderStatus → Player 직접 참조
   - UI_Ping → NetworkManager 직접 참조
   - UI_Ultimate → Player 직접 참조

2. **UIChatManager 책임 과다**
   - 채널 관리 + 백엔드 SDK + UI 로직 혼재
   - 1,168 라인으로 유지보수 어려움

3. **채팅 시스템 버그**
   ```csharp
   // UI_IngameChat.cs:42
   UIChatManager.Instance.SendMessage(text);  // ❌ 메서드가 존재하지 않음
   ```
   - 실제 메서드: `SendChatMessage(string text)`
   - 런타임 오류 발생 가능

#### 개선안
```csharp
// 1. MVP 패턴 적용
public interface IChatView
{
    void DisplayMessage(Message message);
    void ShowError(string error);
    string GetInputText();
}

public class ChatPresenter
{
    private IChatView _view;
    private IChatService _service;

    public ChatPresenter(IChatView view, IChatService service)
    {
        _view = view;
        _service = service;
    }

    public void SendMessage()
    {
        string text = _view.GetInputText();

        _service.SendMessage(text)
            .Subscribe(
                onSuccess: _ => _view.DisplayMessage(new Message(text, "Me")),
                onError: error => _view.ShowError(error.Message)
            );
    }
}

// 2. 이벤트 기반 UI 업데이트
public class UI_GunPowderStatus : MonoBehaviour
{
    private void Start()
    {
        // Player 직접 참조 대신 이벤트 구독
        EventManager.Instance.OnDataChanged += UpdateGunpowderUI;
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnDataChanged -= UpdateGunpowderUI;
    }

    private void UpdateGunpowderUI(int viewId, int current, int max)
    {
        // UI 업데이트 로직만 담당
        gunpowderBar.value = (float)current / max;
    }
}

// 3. 버그 수정
// UI_IngameChat.cs:42 수정
UIChatManager.Instance.SendChatMessage(text);  // ✅ 올바른 메서드
```

---

### 3.5 Account/Auth 모듈

#### 현상
- **AccountManager.cs** (DontDestroySingleton): 로그인, 회원가입, 닉네임 변경 관리
- **AccountRepository.cs**: Firebase + TheBackend SDK 호출 담당
- DDD 패턴 적용 (Domain → Repository → Manager → UI)

#### 문제점
1. **비동기 처리 안전성 부족**
   ```csharp
   // AccountManager.cs:252
   async void DeleteAccount()  // ❌ async void
   {
       await _repository.DeleteAccount();
       // 호출자에서 예외 처리 불가
   }
   ```
   - `async void` 사용으로 예외 처리 불가
   - 연산 완료 확인 어려움

2. **다중 백엔드 SDK 혼재**
   - SetNickname이 Firebase Auth → Firebase Firestore → Backend 순으로 호출
   - 중간 단계 실패 시 부분 업데이트 상태 → 데이터 불일치

3. **이벤트 리스너 관리 부족**
   ```csharp
   // AccountRepository.cs:283
   void ListenForSessionChanges()
   {
       // Firestore listener 시작
       _firestore.Collection("sessions")
           .Document(userId)
           .Listen(snapshot => { });
       // 명시적 정리 없음
   }
   ```
   - Firestore listener가 명시적으로 해제되지 않음
   - 메모리 누수 가능성

#### 개선안
```csharp
// 1. 비동기 처리 개선
public class AccountManager
{
    // ✅ async Task로 변경
    public async Task<Result> DeleteAccountAsync()
    {
        try
        {
            await _repository.DeleteAccountAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            Debug.LogError($"DeleteAccount failed: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}

// 2. 트랜잭션 패턴 적용
public class AccountService
{
    private IFirebaseRepository _firebase;
    private IBackendRepository _backend;

    public async Task<Result> SetNicknameAsync(string nickname)
    {
        using var transaction = new AccountTransaction();

        try
        {
            // 트랜잭션 시작
            await transaction.BeginAsync();

            // Firebase 업데이트
            await _firebase.UpdateNicknameAsync(nickname);

            // Backend 업데이트
            await _backend.UpdateNicknameAsync(nickname);

            // 트랜잭션 커밋
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            // 실패 시 롤백
            await transaction.RollbackAsync();
            return Result.Failure(ex.Message);
        }
    }
}

// 3. 리스너 수명 주기 관리
public class AccountRepository : IDisposable
{
    private ListenerRegistration _sessionListener;

    void ListenForSessionChanges()
    {
        _sessionListener = _firestore.Collection("sessions")
            .Document(userId)
            .Listen(snapshot => { });
    }

    public void Dispose()
    {
        _sessionListener?.Stop();  // 명시적 해제
    }
}
```

---

### 3.6 Chat 모듈

#### 현상
- **UIChatManager.cs** (1,168 라인): 채팅 관리, 채널 동기화, 메시지 송수신
- Backend Chat SDK 사용
- 채팅 메시지 전송, 귓속말, 번역 기능 (미구현)

#### 문제점
1. **God Class**
   - 채널 관리 + SDK 래핑 + UI 로직 혼재
   - 1,168 라인으로 유지보수 어려움

2. **시스템 간 복잡한 의존성**
   - JoinInGameChannel()가 Photon Room Properties + Backend Chat SDK 조합
   - Master vs. Client 조건부 분기 복잡
   - 재시도 코루틴(CoRetryJoinInGameChannel)이 외부 상태 의존

#### 개선안
```csharp
// 1. 책임 분리 (Service + Presenter + View)
public interface IChatService
{
    Task JoinChannelAsync(string channelName);
    Task SendMessageAsync(string message);
    Task SendWhisperAsync(string target, string message);
    IObservable<ChatMessage> OnMessageReceived { get; }
}

public class BackendChatService : IChatService
{
    // Backend Chat SDK만 래핑
    // UI 로직 없음
}

public class IngameChatCoordinator
{
    private IChatService _chatService;
    private IRoomNetworkAdapter _roomAdapter;

    public async Task JoinIngameChannelAsync()
    {
        // 채널 정보를 Photon Room Properties에서 읽기
        var channelInfo = await _roomAdapter.GetChannelInfoAsync();

        // 채팅 서비스에 채널 조인 요청
        await _chatService.JoinChannelAsync(channelInfo.ChannelName);
    }
}

// 2. 재시도 로직 개선
public class ChatRetryHandler
{
    private readonly IChatService _service;
    private readonly CancellationTokenSource _cts;

    public async Task RetryJoinAsync(string channelName, int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _service.JoinChannelAsync(channelName);
                return; // 성공 시 종료
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Debug.Log($"Retry {i + 1}/{maxRetries}: {ex.Message}");
                await Task.Delay(1000 * (i + 1), _cts.Token);
            }
        }
    }
}
```

---

## 4. 데이터 및 의존성 흐름 분석

### 4.1 주요 데이터 추적

#### 4.1.1 Player Data (플레이어 데이터)

**생성 흐름:**
```
[Backend/Firebase] → AccountRepository → AccountManager → PlayerStat → Player
```

**소비 흐름:**
```
Player → PlayerCombat → GameManager (생존 체크)
Player → PlayerVisuals → UI (Gunpowder UI)
Player → EventManager → UI (이벤트 구독)
```

**문제점:**
- 데이터 소스가 단일하지 않음 (Firebase, Backend, CustomProperties)
- 중복 저장 가능성 → 동기화 필요

#### 4.1.2 Room Data (방 데이터)

**생성 흐름:**
```
LobbyManager (방 생성) → Photon CustomProperties → RoomManager
```

**소비 흐름:**
```
RoomManager → Player (팀 할당)
RoomManager → UI (맵 로드)
RoomManager → GameManager (게임 시작)
```

**문제점:**
- CustomProperties가 전역 공유 상태 역할
- 속성 변경 시 즉시 반영되지 않음 (비동기성)
- 타이밍 문제 가능성

#### 4.1.3 Chat Data (채팅 데이터)

**생성 흐름:**
```
UI_IngameChat → UIChatManager → Backend Chat SDK
```

**소비 흐름:**
```
Backend Chat SDK → UIChatManager → UI_IngameChat (메시지 표시)
```

**문제점:**
- 채널 정보를 Photon Room Properties에서 읽음 → 시스템 간 결합
- 재시도 로직이 외부 상태 의존

---

### 4.2 전역 변수(Static) 사용 리스트

#### 싱글톤 패턴 사용 (29개)

**DontDestroySingleton (씬 전환 시 유지):**
```csharp
- EventManager
- ClientManager
- AccountManager
- BuffManager
- ChatManager
- UIChatManager
- CurrencyManager
- FriendManager
- ItemStorage
- PartyManager
- BackendManager
- SceneTransitionManager
- VFXPool
```

**PhotonSingleton (씬 단위):**
```csharp
- GameManager
- LobbyManager
- RoomManager
- GameResultManager
```

**Singleton (일반):**
```csharp
- GoogleLogIn
- UltimateManager
- FallDeadPathManager
- MapDataManager
- BackendChart
- DamageChecker
- RoomStatManager
- ExplosionPool
- FriendManagerLegacy
- PartyManager_Legacy
```

#### 정적 Instance 참조 (11개)
```csharp
public static T Instance { get; private set; }
```

#### 문제점
1. **암묵적 의존성**
   - 클래스가 싱글톤.Instance를 직접 참조 → 의존성 숨겨짐
   - 테스트 시 모킹 어려움

2. **초기화 순서 의존성**
   - GameManager가 RoomManager를, RoomManager가 Player를 참조
   - Awake() 호출 순서에 따라 null 참조 가능

3. **수명 주기 관리 어려움**
   - 씬 전환 시 DontDestroySingleton이 누적
   - 명시적 정리 없음 → 메모리 누수

---

### 4.3 의존성 줄이기 위한 제안

#### 4.3.1 의존성 주입 (Dependency Injection) 도입

```csharp
// 1. 서비스 레지스트리 패턴
public class ServiceRegistry
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public static T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new InvalidOperationException($"Service {typeof(T)} not registered");
    }
}

// 2. 컴포지션 루트 (Composition Root)
public class GameCompositionRoot : MonoBehaviour
{
    void Awake()
    {
        // 서비스 등록
        ServiceRegistry.Register(new EventBus());
        ServiceRegistry.Register(new PhotonNetworkAdapter());
        ServiceRegistry.Register(new ChatService());

        // 매니저 초기화
        var eventBus = ServiceRegistry.Get<IEventBus>();
        var networkAdapter = ServiceRegistry.Get<INetworkAdapter>();

        var gameManager = FindObjectOfType<GameManager>();
        gameManager.Initialize(eventBus, networkAdapter);
    }
}

// 3. 명시적 의존성 주입
public class GameManager : MonoBehaviour
{
    private IEventBus _eventBus;
    private INetworkAdapter _network;

    // ✅ 생성자 주입
    public GameManager(IEventBus eventBus, INetworkAdapter network)
    {
        _eventBus = eventBus;
        _network = network;
    }

    // ✅ 또는 메서드 주입
    public void Initialize(IEventBus eventBus, INetworkAdapter network)
    {
        _eventBus = eventBus;
        _network = network;
    }
}
```

#### 4.3.2 전역 상태 중앙화

```csharp
// 1. 중앙 상태 스토어 (Redux-like)
public interface IGameStateStore
{
    T GetState<T>() where T : class, new();
    void UpdateState<T>(Action<T> updater);
    IObservable<T> ObserveState<T>() where T : class, new();
}

public class GameStateStore : IGameStateStore
{
    private readonly Dictionary<Type, object> _states = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Subject<object>> _subjects = new Dictionary<Type, Subject<object>>();

    public T GetState<T>() where T : class, new()
    {
        if (!_states.TryGetValue(typeof(T), out var state))
        {
            state = new T();
            _states[typeof(T)] = state;
        }
        return (T)state;
    }

    public void UpdateState<T>(Action<T> updater)
    {
        var state = GetState<T>();
        updater(state);

        // 상태 변경 알림
        if (_subjects.TryGetValue(typeof(T), out var subject))
        {
            subject.OnNext(state);
        }
    }

    public IObservable<T> ObserveState<T>() where T : class, new()
    {
        if (!_subjects.ContainsKey(typeof(T)))
        {
            _subjects[typeof(T)] = new Subject<object>();
        }
        return _subjects[typeof(T)].Cast<T>().AsObservable();
    }
}

// 2. 상태 정의
public class PlayerState
{
    public int Gunpowder { get; set; }
    public int Life { get; set; }
    public EInGameTeam Team { get; set; }
}

public class RoomState
{
    public EMap SelectedMap { get; set; }
    public int PlayerCount { get; set; }
    public bool IsGameStarted { get; set; }
}

// 3. 사용 예시
public class UI_GunpowderStatus : MonoBehaviour
{
    private IGameStateStore _stateStore;

    void Start()
    {
        _stateStore = ServiceRegistry.Get<IGameStateStore>();

        // 상태 변경 관찰
        _stateStore.ObserveState<PlayerState>()
            .Subscribe(state => UpdateUI(state));
    }

    void UpdateUI(PlayerState state)
    {
        gunpowderBar.value = (float)state.Gunpowder / 100f;
    }
}
```

#### 4.3.3 싱글톤 사용 제한

**허용되는 싱글톤 (Core Services):**
```csharp
- EventBus (이벤트 버스)
- ServiceRegistry (서비스 레지스트리)
- GameStateStore (상태 스토어)
```

**비추천 싱글톤 → 의존성 주입으로 변경:**
```csharp
- GameManager → GameFlowCoordinator
- AccountManager → AccountService
- RoomManager → RoomService
- Player → PlayerController (게임 오브젝트)
```

---

## 5. 확장성 (Extensibility) 분석

### 5.1 좋은 확장성 패턴

#### 5.1.1 Bomb 시스템 (상속 기반)

```csharp
// ✅ 좋은 패턴
public abstract class Bomb : MonoBehaviourPun, IBomb
{
    protected BombStat _stat;

    public virtual void Explode()
    {
        // 기본 폭발 로직
    }
}

// 확장 용이
public class BounceBomb : Bomb
{
    public override void Explode()
    {
        // 튕기는 폭탄 특정 로직
        base.Explode(); // 기본 로직 재사용
    }
}

public class MissileBomb : Bomb
{
    // 미사일 특정 로직
}
```

**장점:**
- OCP 준수 (확장에는 열려, 수정에는 닫혀)
- 새 폭탄 타입 추가 시 기존 코드 수정 불필요

---

### 5.2 개선 필요한 확장성

#### 5.2.1 아이템/스킨 시스템

**현재 구현:**
```csharp
// ❌ OCP 위반
public void LoadItems()
{
    var bomb = ItemStorage.Instance.GetItem(EItemType.Bomb);
    var head = ItemStorage.Instance.GetItem(EItemType.Head);
    // ...

    switch (bomb.ID)
    {
        case 1: // 하드코딩
            // 처리
            break;
        case 2:
            // 처리
            break;
        // 새 아이템 추가 시 case 추가 필요
    }
}
```

**개선안:**
```csharp
// ✅ 팩토리 패턴 + ScriptableObject
public interface IItem
{
    int ID { get; }
    void Apply(Player player);
}

public interface IItemFactory
{
    IItem Create(ItemData data);
}

public class BombItemFactory : IItemFactory
{
    public IItem Create(ItemData data)
    {
        return data.BombType switch
        {
            EBombType.Basic => new BasicBombItem(data),
            EBombType.Bounce => new BounceBombItem(data),
            EBombType.Missile => new MissileBombItem(data),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

// ScriptableObject 기반 데이터
[CreateAssetMenu]
public class BombData : ScriptableObject
{
    public int id;
    public EBombType type;
    public int damage;
    public float fuseTime;
    // ...
}

// 확장 용이
[CreateAssetMenu]
public class BounceBombData : BombData
{
    public int bounceCount;
}
```

#### 5.2.2 스킬/버프 시스템

**현재 구현:**
```csharp
// ❌ 하드코딩
public void ApplyBuff(BuffType type)
{
    switch (type)
    {
        case BuffType.Speed:
            // 속도 증가
            break;
        case BuffType.Damage:
            // 데미지 증가
            break;
        // 새 버프 추가 시 case 추가 필요
    }
}
```

**개선안:**
```csharp
// ✅ 전략 패턴
public interface IBuffStrategy
{
    void Apply(Player player);
    void Remove(Player player);
}

public class SpeedBuffStrategy : IBuffStrategy
{
    public void Apply(Player player) => player.MovementSpeed *= 1.5f;
    public void Remove(Player player) => player.MovementSpeed /= 1.5f;
}

public class DamageBuffStrategy : IBuffStrategy
{
    public void Apply(Player player) => player.DamageMultiplier *= 1.2f;
    public void Remove(Player player) => player.DamageMultiplier /= 1.2f;
}

public class BuffRegistry
{
    private readonly Dictionary<BuffType, IBuffStrategy> _strategies = new Dictionary<BuffType, IBuffStrategy>();

    public BuffRegistry()
    {
        _strategies[BuffType.Speed] = new SpeedBuffStrategy();
        _strategies[BuffType.Damage] = new DamageBuffStrategy();
        // 새 버프 추가: 여기에 등록만
    }

    public IBuffStrategy GetStrategy(BuffType type)
    {
        return _strategies[type];
    }
}
```

---

## 6. 리팩토링 로드맵 (90일 기준)

### Phase 1: 긴급 버그 수정 및 안전장치 (1-2주)

**목표:**
- UI_IngameChat.cs 메서드 이름 수정
- `async void` → `async Task` 변경
- 리스너 수명 주기 관리 추가

**작업:**
1. UI_IngameChat.cs: `SendMessage` → `SendChatMessage`
2. AccountManager.DeleteAccount → `async Task`로 변경
3. AccountRepository 리스너 `Dispose()` 패턴 추가
4. RoomManager `UniTaskVoid` → `UniTask` 변경

---

### Phase 2: Player 모듈 리팩토링 (3-4주)

**목표:**
- Player.cs 책임 분리
- 하위 시스템 조합 패턴 적용
- 상수화 및 설정 분리

**작업:**
1. Player 하위 시스템 생성 (PlayerCombat, PlayerMovement, PlayerVisuals, PlayerNetworkBridge)
2. 상태 전환 로직 Player에서 하위 시스템으로 이동
3. PlayerConstants 정의 및 매직 넘버 제거
4. 타이밍 동기화 개선 (PhotonNetwork.Time 사용)
5. 단위 테스트 작성 (하위 시스템별)

---

### Phase 3: 네트워크 레이어 분리 (3-4주)

**목표:**
- Photon 콜백과 게임 로직 분리
- 네트워크 어댑터 패턴 적용
- 상태 소유권 명확화

**작업:**
1. INetworkAdapter 인터페이스 정의
2. PhotonNetworkAdapter 구현 (콜백 → Observable 변환)
3. GameRoomCoordinator 생성 (게임 로직만 담당)
4. CustomProperties 동기화 개선
5. 통합 테스트 (네트워크 시뮬레이션)

---

### Phase 4: UI 모듈 MVP 패턴 적용 (2-3주)

**목표:**
- UI와 로직 분리
- 이벤트 기반 UI 업데이트
- 채팅 시스템 리팩토링

**작업:**
1. IChatService, ChatPresenter, IChatView 인터페이스 정의
2. UI_GunpowderStatus, UI_Ping, UI_Ultimate 이벤트 구독으로 변경
3. UIChatManager 책임 분리 (채널 관리, SDK, UI)
4. 채팅 재시도 로직 개선

---

### Phase 5: 의존성 주입 도입 (3-4주)

**목표:**
- ServiceRegistry 패턴 적용
- 전역 싱글톤 감축
- GameStateStore 중앙화

**작업:**
1. ServiceRegistry 구현
2. GameCompositionRoot 생성
3. 핵심 서비스 등록 (EventBus, NetworkAdapter, ChatService)
4. GameManager, AccountManager 의존성 주입으로 변경
5. GameStateStore 구현 및 상태 관리 개선

---

### Phase 6: 확장성 개선 (3-4주)

**목표:**
- 팩토리 패턴 적용 (아이템, 스킬)
- 전략 패턴 적용 (버프)
- ScriptableObject 데이터 구조화

**작업:**
1. IItemFactory 인터페이스 및 구현
2. IBuffStrategy 인터페이스 및 구현
3. BombData, ItemData ScriptableObject 정의
4. 하드코딩 제거 (switch 문 → 팩토리/전략 패턴)
5. 새 기능 추가 가이드 문서화

---

## 7. 결론 및 권장 사항

### 7.1 핵심 문제 요약

| 문제 카테고리 | 심각도 | 영향 범위 |
| :--- | :--- | :--- |
| **God Class** | 🔴 높음 | Player.cs (1,552 라인), UIChatManager.cs (1,168 라인) |
| **Tight Coupling** | 🔴 높음 | 네트워크 콜백 ↔ 게임 로직, UI ↔ 플레이어 |
| **Global State** | 🟠 중간 | 29개 싱글톤 클래스 |
| **Async Safety** | 🟠 중간 | `async void`, `UniTaskVoid` 사용 |
| **Extensibility** | 🟡 낮음 | 아이템/스킬 시스템 하드코딩 |
| **Code Duplication** | 🟡 낮음 | 유사 로직 반복 |

---

### 7.2 우선순위 기반 권장 사항

**즉시 조치 (1-2주):**
1. UI_IngameChat.cs 버그 수정
2. `async void` → `async Task` 변경
3. 리스너 수명 주기 관리 추가

**단기 개선 (1-2개월):**
1. Player 모듈 리팩토링 (최우선)
2. 네트워크 레이어 분리
3. UI 모듈 MVP 패턴 적용

**중기 개선 (2-3개월):**
1. 의존성 주입 도입
2. 확장성 개선 (팩토리/전략 패턴)
3. 전역 상태 중앙화

---

### 7.3 최종 권장 사항

**아키텍처 철학:**
1. **명확한 계층 분리**: Presentation (UI) → Application (Manager) → Domain (Logic) → Infrastructure (Network/Data)
2. **단일 책임 원칙 준수**: 각 클래스는 하나의 명확한 책임만 가져야 함
3. **의존성 역전**: 상위 모듈은 하위 모듈에 의존하지 않음 (인터페이스 활용)
4. **전역 상태 최소화**: 싱글톤은 핵심 서비스로만 제한

**개발 프로세스:**
1. **코드 리뷰 강화**: God Class, 매직 넘버, Tight Coupling 자동 감지
2. **아키텍처 문서화**: 모듈 간 의존성 다이어그램 유지보수
3. **테스트 주도 개발(TDD)**: 리팩토링 시 단위 테스트 필수
4. **기능 추가 가이드**: 새 기능 추가 시 OCP 준수 확인

**성능 최적화:**
1. **오브젝트 풀링 확대**: Bomb, GunPowder, DamagePopup
2. **SendRate 조정**: 필요 시 20으로 낮춰 네트워크 트래픽 감소
3. **이벤트 버스 최적화**: 핫패스 (HotPath) 이벤트 식별

---

## 8. 참고 자료

### 8.1 주요 파일 경로

**핵심 Manager:**
- `Assets/02.Scripts/GameManager.cs` (423 라인)
- `Assets/02.Scripts/Photon/PhotonServerManager.cs`
- `Assets/02.Scripts/Event/EventManager.cs`

**Player 시스템:**
- `Assets/02.Scripts/Player/Player.cs` (1,552 라인) 🔴
- `Assets/02.Scripts/Player/PlayerStat.cs` (593 라인)
- `Assets/02.Scripts/Player/State/PlayerFSM.cs`

**Network:**
- `Assets/02.Scripts/Photon/WaitingRoom/RoomManager.cs` 🔴
- `Assets/02.Scripts/Photon/Lobby/LobbyManager.cs`

**Chat:**
- `Assets/02.Scripts/Chat/UIChatManager.cs` (1,168 라인) 🔴
- `Assets/02.Scripts/UI/KillLog/UI_IngameChatPopup.cs` (819 라인)
- `Assets/02.Scripts/UI/Ingame/UI_IngameChat.cs` ⚠️ (버그)

**Bomb:**
- `Assets/02.Scripts/Bomb/Bomb.cs`

---

### 8.2 참조된 디자인 패턴

1. **Singleton Pattern**: 현재 29개 싱글톤 사용 → 의존성 주입으로 대체 권장
2. **State Pattern**: PlayerFSM에서 사용 ✅ (확장 가능)
3. **Observer Pattern**: EventManager에서 사용 ✅ (전역 오남용 주의)
4. **Factory Pattern**: Bomb 시스템 ✅ (아이템/스킬 시스템으로 확장 권장)
5. **Strategy Pattern**: 버프 시스템에 적용 권장
6. **MVP Pattern**: UI 모듈에 적용 권장
7. **Adapter Pattern**: 네트워크 레이어에 적용 권장
8. **Composition Pattern**: Player 하위 시스템 조합에 적용 권장

---

### 8.3 외부 라이브러리

- **Photon PUN2**: 멀티플레이어 네트워킹
- **TheBackend SDK**: 백엔드 통신
- **Backend Chat SDK**: 채팅 시스템
- **Firebase**: 인증, 데이터베이스
- **RobustFSM**: 상태 머신 라이브러리
- **DOTween**: 애니메이션

---

**문서 작성일**: 2026년 1월 26일
**버전**: 1.0
**작성자**: Sisyphus Architecture Analysis
