# 실전 리팩토링 가이드 문서

> **문서 버전**: 1.0
> **작성일**: 2026년 1월 26일
> **대상**: GunPowder 프로젝트 개발팀
> **기반 문서**: Technical_Debt_Architecture_Analysis.md

---

## 📋 개요

이 문서는 앞서 분석된 기술 부채를 바탕으로, 팀원들이 **이해하고 바로 실행할 수 있는 실전 가이드**를 제공합니다. 이론적인 설명보다는 **실제 코드 예시**와 **구체적인 지침**을 중심으로 작성되었습니다.

**핵심 목표:**
1. 분석된 문제가 실제 코드 어디에 있는지 **증거로 증명**
2. 리팩토링 후 같은 문제 재발 방지를 위한 **팀 규칙** 정립
3. 가장 시급한 `Player.cs` 분해를 위한 **단계별 지침** 제공

---

## Part 1. 팩트 체크 및 문제 증명 (Verification)

팀원들을 설득하기 위해, 분석 보고서에서 지적한 문제가 **실제 코드 어디에 있는지 증거**를 보여드립니다.

### 1.1 비동기 위험 (Async Void)

**실제 코드 예시:**

```csharp
// ❌ Bad Case (현재 코드)
// 파일: Assets/02.Scripts/Account/3. Manager/AccountManager.cs:252
public async void DeleteAccount()
{
    await _accountRepository.DeleteAccount();
    Logout();
}

// 파일: Assets/02.Scripts/Account/4. UI/UI_LoginScene.cs:201
public async void Login()
{
    // 로그인 로직...
}
```

**문제점 분석:**

| 항목 | 설명 |
|------|------|
| **예외 처리 불가** | `async void`는 호출자에서 `try-catch`로 예외를 잡을 수 없습니다. |
| **완료 확인 불가** | 호출자는 연산이 언제 완료되는지 알 수 없습니다. |
| **비동기 누수** | 예외 발생 시 로그가 사라지거나 앱 크래시가 발생합니다. |

```csharp
// ⚠️ 이 코드는 왜 try-catch로 예외를 잡을 수 없는가?

// 호출 코드
try
{
    accountManager.DeleteAccount();  // ❌ 여기서 예외가 catch 되지 않음!
}
catch (Exception ex)
{
    // 이 코드는 실행되지 않습니다.
    Debug.LogError($"삭제 실패: {ex.Message}");
}
```

**이유:** `async void` 메서드에서 발생한 예외는 **SynchronizationContext**로 전달되지 않고, 호출 스택에서 사라집니다. 이는 `.NET`의 설계 결정입니다.

---

### 1.2 결합도 증명 (Player.cs의 강한 결합)

**실제 코드 예시:**

```csharp
// ❌ Bad Case (현재 코드)
// 파일: Assets/02.Scripts/Player/Player.cs

// 라인 249
private void OnDestroy()
{
    if (EventManager.Instance != null)  // 싱글톤 직접 참조
    {
        EventManager.Instance.OnPlayerItemChanged -= LoadItems;  // 직접 이벤트 구독 해제
    }
}

// 라인 315-317
private void LoadUltimate()
{
    if (UltimateManager.Instance != null && _ultimateController != null)  // 싱글톤 직접 참조
    {
        Ultimate ultimate = UltimateManager.Instance.GetUltimate(
            EquipedItemDict[EItemType.Bomb].ID,  // 싱글톤에서 데이터 직접 가져옴
            this
        );
    }
}

// 라인 404
private void Start()
{
    if (EventManager.Instance != null)  // 싱글톤 직접 참조
    {
        EventManager.Instance.OnPlayerItemChanged += LoadItems;  // 직접 이벤트 구독
    }
}
```

**현재의 강한 결합 상태:**

```
Player (비즈니스 로직)
    ↓ 직접 참조 (Tight Coupling)
EventManager (전역 이벤트 버스)
    ↓ 직접 참조
UltimateManager (전역 매니저)
    ↓ 직접 참조
ItemStorage (전역 매니저)
```

**문제점:**

| 문제 | 설명 |
|------|------|
| **테스트 불가** | Player 클래스를 테스트하려면 EventManager, UltimateManager 등 모든 싱글톤이 초기화되어 있어야 함 |
| **수명 주기 의존** | EventManager가 먼저 파괴되면 Player.OnDestroy에서 NullReference 발생 |
| **순환 의존성** | Player → EventManager → GameManager → Player (순환) |
| **수정 영향 범위** | Player를 수정하면 참조하는 모든 매니저 영향 |

---

## Part 2. 팀 코딩 컨벤션 (Team Conventions)

리팩토링 이후 **같은 문제가 재발하지 않도록** 새로운 규칙을 정의합니다.

### 2.1 Singleton 사용 금지

**규칙:**
- 앞으로 `XXXManager.Instance` 접근을 **금지**합니다.
- `Init(Manager manager)` 같은 주입 방식이나 `ServiceLocator` 패턴을 사용합니다.

**❌ Bad Case (금지될 패턴):**

```csharp
// ❌ 금지: 직접 싱글톤 접근
public class Player : MonoBehaviour
{
    void TakeDamage(int damage)
    {
        GameManager.Instance.SetPlayerDead(PhotonView.ViewID);  // 직접 접근
    }

    void Start()
    {
        EventManager.Instance.OnPlayerItemChanged += LoadItems;  // 직접 접근
    }
}
```

**✅ Good Case (권장 패턴):**

```csharp
// ✅ 권장 1: 생성자/메서드 주입
public class Player : MonoBehaviour
{
    private IGameEventManager _eventManager;
    private IGameFlowManager _gameFlow;

    // 생성자 주입 (또는 Init 메서드)
    public void Init(IGameEventManager eventManager, IGameFlowManager gameFlow)
    {
        _eventManager = eventManager;
        _gameFlow = gameFlow;

        // 이벤트 구독
        _eventManager.OnPlayerItemChanged += LoadItems;
    }

    void OnDestroy()
    {
        // 주입받은 인터페이스로 해제
        _eventManager.OnPlayerItemChanged -= LoadItems;
    }

    void TakeDamage(int damage)
    {
        // 주입받은 인터페이스로 호출
        _gameFlow.SetPlayerDead(PhotonView.ViewID);
    }
}

// ✅ 권장 2: Service Locator 패턴
public class ServiceLocator
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public static T Get<T>()
    {
        return (T)_services[typeof(T)];
    }
}

public class Player : MonoBehaviour
{
    private IGameEventManager _eventManager;

    void Start()
    {
        // Service Locator에서 가져오기
        _eventManager = ServiceLocator.Get<IGameEventManager>();
        _eventManager.OnPlayerItemChanged += LoadItems;
    }
}

// ✅ 권장 3: Composition Root (최상위에서 주입)
public class GameCompositionRoot : MonoBehaviour
{
    void Awake()
    {
        // 서비스 등록
        var eventManager = new GameEventManager();
        var gameFlow = new GameFlowManager();

        ServiceLocator.Register(eventManager);
        ServiceLocator.Register(gameFlow);

        // Player 초기화
        var player = FindObjectOfType<Player>();
        player.Init(eventManager, gameFlow);
    }
}
```

**체크리스트:**
- [ ] `XXXManager.Instance`를 직접 참조하는 코드가 없는지?
- [ ] 인터페이스를 통한 의존성 주입을 사용하는지?
- [ ] 단위 테스트 시 모킹이 가능한지?

---

### 2.2 Async/Await 규칙

**규칙:**
- `async void`를 **금지**합니다.
- `async UniTask` 또는 `async Task`를 의무적으로 사용합니다.
- 비동기 메서드명은 반드시 `Async`로 끝냅니다.

**❌ Bad Case (금지될 패턴):**

```csharp
// ❌ 금지: async void
public async void DeleteAccount()
{
    await _repository.DeleteAccount();
    // 예외가 catch 되지 않음!
}

// ❌ 금지: 메서드명이 Async로 끝나지 않음
public async Task Login(string id, string password)
{
    // 로그인 로직...
}
```

**✅ Good Case (권장 패턴):**

```csharp
// ✅ 권장 1: async Task 사용
public async Task DeleteAccountAsync()
{
    try
    {
        await _repository.DeleteAccountAsync();
        Logout();
    }
    catch (Exception ex)
    {
        // 예외 처리 가능!
        Debug.LogError($"계정 삭제 실패: {ex.Message}");
        throw;
    }
}

// ✅ 권장 2: async UniTask 사용 (UniTask 라이브러리 사용 시)
public async UniTaskVoid DeleteAccountAsync()
{
    try
    {
        await _repository.DeleteAccountAsync();
        Logout();
    }
    catch (Exception ex)
    {
        Debug.LogError($"계정 삭제 실패: {ex.Message}");
        throw;
    }
}

// ✅ 권장 3: 호출자에서 예외 처리 가능
// 호출 코드
try
{
    await accountManager.DeleteAccountAsync();
}
catch (Exception ex)
{
    // ✅ 예외가 catch 됨!
    ShowError("계정 삭제 실패", ex.Message);
}
```

**메서드 명명 규칙:**

| 작업 | ❌ Bad | ✅ Good |
|------|---------|---------|
| 데이터 가져오기 | `GetData()` | `GetDataAsync()` |
| 저장 | `Save()` | `SaveAsync()` |
| 계정 삭제 | `DeleteAccount()` | `DeleteAccountAsync()` |
| 로그인 | `Login()` | `LoginAsync()` |

**체크리스트:**
- [ ] 모든 비동기 메서드가 `async Task` 또는 `async UniTask`로 선언되어 있는지?
- [ ] 비동기 메서드명이 `~Async`로 끝나는지?
- [ ] 호출자에서 `try-catch`로 예외를 잡을 수 있는지?

---

### 2.3 데이터-로직 분리

**규칙:**
- **데이터 클래스(`Data`)**에는 로직을 넣지 않습니다.
- **뷰(`UI`)**는 로직(`Manager`)을 직접 참조하지 않고 **`Event`**를 씁니다.

**❌ Bad Case (금지될 패턴):**

```csharp
// ❌ 금지 1: 데이터 클래스에 로직 포함
public class PlayerData
{
    public int Health;
    public int Gunpowder;

    // ❌ 데이터 클래스에 비즈니스 로직이 포함됨
    public void DecreaseHealth(int amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            Debug.Log("플레이어 사망!");
            // 로직이 데이터 클래스에 섞임
        }
    }
}

// ❌ 금지 2: UI가 로직을 직접 참조
public class UI_GunPowderStatus : MonoBehaviour
{
    private Player _player;  // ❌ UI가 직접 비즈니스 로직 참조

    void Start()
    {
        _player = FindObjectOfType<Player>();
        _player.OnHealthChanged += UpdateUI;  // ❌ 직접 이벤트 구독
    }

    void OnDamageButtonClick()
    {
        _player.TakeDamage(10);  // ❌ UI가 로직 직접 호출
    }
}
```

**✅ Good Case (권장 패턴):**

```csharp
// ✅ 권장 1: 데이터 클래스는 순수 데이터만
public class PlayerData
{
    public int Health { get; set; }
    public int Gunpowder { get; set; }
    public EInGameTeam Team { get; set; }

    // ✅ 로직 없음 (데이터만)
}

// ✅ 권장 2: 로직은 별도 클래스에서 처리
public class PlayerHealthManager : MonoBehaviour
{
    private PlayerData _data;

    public PlayerHealthManager(PlayerData data)
    {
        _data = data;
    }

    public void DecreaseHealth(int amount)
    {
        _data.Health -= amount;

        if (_data.Health <= 0)
        {
            // 이벤트로 알림 (데이터 변경 알림)
            _onPlayerDied?.Invoke();
        }
    }

    private event Action _onPlayerDied;
    public event Action OnPlayerDied => _onPlayerDied;
}

// ✅ 권장 3: UI는 이벤트만 구독 (로직 직접 참조 X)
public class UI_GunPowderStatus : MonoBehaviour
{
    private IGameEventBus _eventBus;  // ✅ 이벤트 버스만 참조

    void Start()
    {
        _eventBus = ServiceLocator.Get<IGameEventBus>();

        // ✅ 이벤트만 구독 (플레이어 직접 참조 X)
        _eventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }

    void OnHealthChanged(PlayerHealthChangedEvent evt)
    {
        // ✅ 이벤트 데이터로 UI 업데이트
        healthBar.value = (float)evt.CurrentHealth / evt.MaxHealth;
    }

    void OnDamageButtonClick()
    {
        // ✅ 이벤트로 요청 (직접 호출 X)
        _eventBus.Publish(new DamageRequestEvent(10));
    }
}

// ✅ 이벤트 데이터 정의
public class PlayerHealthChangedEvent
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }

    public PlayerHealthChangedEvent(int current, int max)
    {
        CurrentHealth = current;
        MaxHealth = max;
    }
}

public class DamageRequestEvent
{
    public int Damage { get; }

    public DamageRequestEvent(int damage)
    {
        Damage = damage;
    }
}
```

**체크리스트:**
- [ ] 데이터 클래스에 메서드(로직)가 포함되지 않는지?
- [ ] UI 클래스가 Manager 클래스를 직접 참조하지 않는지?
- [ ] UI는 이벤트만 구독/발행하는지?
- [ ] 데이터 변경은 이벤트로 알리는지?

---

## Part 3. Player.cs 분해 실전 가이드 (Action Plan)

가장 비대한 `Player.cs`(1,500라인+)를 리팩토링하기 위한 단계별 지침입니다.

### 3.1 인터페이스 추출

**목적:**
- 외부에서 플레이어의 상태를 참조할 때 `Player` 클래스 전체가 아니라, 필요한 인터페이스만 보도록 설계합니다.

**❌ Bad Case (현재):**

```csharp
// ❌ 현재: 외부에서 Player 클래스 전체를 참조
public class UI_GunPowderStatus : MonoBehaviour
{
    private Player _player;  // 전체 Player 클래스 참조 (불필요한 의존성)

    void Start()
    {
        _player = FindObjectOfType<Player>();
    }

    void UpdateUI()
    {
        // 체력만 필요한데 전체 클래스 참조
        healthBar.value = (float)_player.Stat.CurrentHealth / _player.Stat.MaxHealth;
    }
}
```

**✅ Good Case (권장):**

```csharp
// ✅ 1단계: 인터페이스 정의
public interface IPlayerHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    event Action<int> OnHealthChanged;
}

public interface IPlayerCombat
{
    void TakeDamage(int damage, int attackerViewId);
    void FireBomb(Vector2 direction, EBombType type);
}

public interface IPlayerMovement
{
    void Move(Vector2 input);
    void Jump();
}

// ✅ 2단계: Player가 인터페이스 구현
public class Player : MonoBehaviourPun, IDamagable, IPlayerHealth, IPlayerCombat, IPlayerMovement
{
    // IPlayerHealth 구현
    public int CurrentHealth => _playerStat.CurrentHealth;
    public int MaxHealth => _playerStat.MaxHealth;
    public event Action<int> OnHealthChanged;

    // IPlayerCombat 구현
    public void TakeDamage(int damage, int attackerViewId)
    {
        _combat.TakeDamage(damage, attackerViewId);
    }

    public void FireBomb(Vector2 direction, EBombType type)
    {
        _combat.FireBomb(direction, type);
    }

    // IPlayerMovement 구현
    public void Move(Vector2 input)
    {
        _movement.Move(input);
    }

    public void Jump()
    {
        _movement.Jump();
    }
}

// ✅ 3단계: 외부에서 필요한 인터페이스만 참조
public class UI_GunPowderStatus : MonoBehaviour
{
    private IPlayerHealth _playerHealth;  // ✅ 필요한 인터페이스만 참조

    void Start()
    {
        // ✅ 인터페이스만 주입
        var player = FindObjectOfType<Player>();
        _playerHealth = player;

        _playerHealth.OnHealthChanged += UpdateUI;
    }

    void UpdateUI(int currentHealth)
    {
        healthBar.value = (float)currentHealth / _playerHealth.MaxHealth;
    }

    void OnDestroy()
    {
        _playerHealth.OnHealthChanged -= UpdateUI;
    }
}

// ✅ 4단계: 네트워크 동기화 인터페이스
public interface IPlayerNetwork
{
    void RpcTakeDamage(int damage, int attackerViewId);
    void RpcChangeState(string stateName);
}

public class PlayerNetworkBridge : MonoBehaviourPun, IPlayerNetwork
{
    private Player _owner;

    public void Init(Player owner)
    {
        _owner = owner;
    }

    // 네트워크 동기화만 담당
    [PunRPC]
    public void RpcTakeDamage(int damage, int attackerViewId)
    {
        if (PhotonView.IsMine)
        {
            _owner.TakeDamage(damage, attackerViewId);
        }
    }

    [PunRPC]
    public void RpcChangeState(string stateName)
    {
        _owner.FSM.ChangeState(stateName);
    }
}
```

---

### 3.2 컴포넌트 분리 순서

**3단계 접근:**
- 1단계: 데이터(`PlayerStat`)로 완전 분리
- 2단계: 로직 클래스(`PlayerMovement`, `PlayerCombat`)로 분리
- 3단계: `Player`는 초기화(`Init`)와 연결만 담당하는 **Facade(창구)** 역할만 남기기

#### 1단계: PlayerStat으로 데이터 완전 분리

**❌ Bad Case (현재):**

```csharp
// ❌ 현재: PlayerStat에 로직이 포함
public class PlayerStat : MonoBehaviour
{
    private int _currentHealth;

    public void DecreaseHealth(int amount, int attacker)
    {
        _currentHealth -= amount;

        // ❌ 로직이 데이터 클래스에 포함
        if (_currentHealth <= 0)
        {
            // 궁극기 체크 로직
            if (_currentHealth <= _ultimateTriggerThreshold)
            {
                _hasUltimateChance = true;
            }

            // 이벤트 발행
            EventManager.Instance.OnPlayerDied?.Invoke();
        }

        // 공격자 기록
        _lastAttacker = attacker;
        _lastAttackTime = Time.time;
    }
}
```

**✅ Good Case (권장):**

```csharp
// ✅ 1단계: 데이터 클래스 (순수 데이터)
public class PlayerData
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentGunpowder { get; set; }
    public int MaxGunpowder { get; set; }
    public EInGameTeam Team { get; set; }
    public int LastAttacker { get; set; }
    public bool HasUltimateChance { get; set; }
}

// ✅ 2단계: 데이터 관리자 (데이터 조작 로직만)
public class PlayerDataManager : MonoBehaviour
{
    private PlayerData _data;
    private IGameEventBus _eventBus;

    public PlayerDataManager(IGameEventBus eventBus)
    {
        _data = new PlayerData();
        _eventBus = eventBus;
    }

    public void DecreaseHealth(int amount)
    {
        _data.CurrentHealth = Mathf.Max(0, _data.CurrentHealth - amount);

        // ✅ 이벤트로 알림 (로직은 별도)
        _eventBus.Publish(new PlayerHealthChangedEvent(_data.CurrentHealth, _data.MaxHealth));

        if (_data.CurrentHealth <= 0)
        {
            _eventBus.Publish(new PlayerDiedEvent(_data.LastAttacker));
        }
    }

    public void DecreaseGunpowder(int amount)
    {
        _data.CurrentGunpowder = Mathf.Max(0, _data.CurrentGunpowder - amount);

        _eventBus.Publish(new PlayerGunpowderChangedEvent(_data.CurrentGunpowder, _data.MaxGunpowder));
    }

    public PlayerData GetData() => _data;
}
```

#### 2단계: PlayerMovement, PlayerCombat으로 로직 분리

**✅ Good Case (권장):**

```csharp
// ✅ 2단계-A: 이동 로직 분리
public class PlayerMovement : MonoBehaviour
{
    private PlayerData _data;
    private PlayerSettings _settings;

    private Rigidbody2D _rb2d;
    private GroundChecker _groundChecker;

    public void Init(PlayerData data, PlayerSettings settings)
    {
        _data = data;
        _settings = settings;

        _rb2d = GetComponent<Rigidbody2D>();
        _groundChecker = GetComponent<GroundChecker>();
    }

    public void Move(Vector2 input)
    {
        if (_groundChecker.IsGrounded())
        {
            _rb2d.velocity = new Vector2(
                input.x * _settings.MoveSpeed,
                _rb2d.velocity.y
            );
        }
        else
        {
            // 공중에서 이동 속도 감소
            _rb2d.velocity = new Vector2(
                input.x * _settings.MoveSpeed * 0.6f,
                _rb2d.velocity.y
            );
        }
    }

    public void Jump()
    {
        if (_groundChecker.IsGrounded())
        {
            _rb2d.velocity = new Vector2(
                _rb2d.velocity.x,
                _settings.JumpPower
            );
        }
    }
}

// ✅ 2단계-B: 전투 로직 분리
public class PlayerCombat : MonoBehaviour
{
    private PlayerData _data;
    private IBombFactory _bombFactory;
    private IGameEventBus _eventBus;

    public void Init(PlayerData data, IBombFactory bombFactory, IGameEventBus eventBus)
    {
        _data = data;
        _bombFactory = bombFactory;
        _eventBus = eventBus;
    }

    public void TakeDamage(int damage, int attackerViewId)
    {
        // ✅ 데이터 관리자에 위임
        var dataManager = GetComponent<PlayerDataManager>();
        dataManager.DecreaseHealth(damage);

        // ✅ 공격자 기록
        _data.LastAttacker = attackerViewId;
    }

    public void FireBomb(Vector2 direction, EBombType type)
    {
        if (_data.CurrentGunpowder <= 0) return;

        // ✅ 팩토리에서 폭탄 생성
        Bomb bomb = _bombFactory.Create(type, transform);
        bomb.Launch(direction);

        // ✅ 건파우더 감소
        var dataManager = GetComponent<PlayerDataManager>();
        dataManager.DecreaseGunpowder(bomb.GetCost());
    }
}

// ✅ 2단계-C: 폭탄 팩토리
public interface IBombFactory
{
    Bomb Create(EBombType type, Transform owner);
}

public class BombFactory : IBombFactory
{
    private Dictionary<EBombType, GameObject> _bombPrefabs;

    public BombFactory(Dictionary<EBombType, GameObject> prefabs)
    {
        _bombPrefabs = prefabs;
    }

    public Bomb Create(EBombType type, Transform owner)
    {
        if (_bombPrefabs.TryGetValue(type, out var prefab))
        {
            GameObject bombObj = Instantiate(prefab, owner.position, owner.rotation);
            return bombObj.GetComponent<Bomb>();
        }

        throw new ArgumentException($"Unknown bomb type: {type}");
    }
}
```

#### 3단계: Player는 Facade 역할만

**✅ Good Case (최종):**

```csharp
// ✅ 3단계: Player는 Facade 역할만
public class Player : MonoBehaviourPun, IDamagable,
    IPlayerHealth, IPlayerCombat, IPlayerMovement
{
    // 하위 시스템 (조합 패턴)
    public PlayerDataManager DataManager { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerVisuals Visuals { get; private set; }
    public PlayerNetworkBridge Network { get; private set; }

    private IBombFactory _bombFactory;
    private IGameEventBus _eventBus;

    // ✅ 외부에서 데이터 접근 시 인터페이스만 노출
    public int CurrentHealth => DataManager.GetData().CurrentHealth;
    public int MaxHealth => DataManager.GetData().MaxHealth;

    void Awake()
    {
        // ✅ 하위 시스템 GetComponent로 연결
        DataManager = GetComponent<PlayerDataManager>();
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Visuals = GetComponent<PlayerVisuals>();
        Network = GetComponent<PlayerNetworkBridge>();
    }

    void Start()
    {
        // ✅ 의존성 주입
        _eventBus = ServiceLocator.Get<IGameEventBus>();
        _bombFactory = ServiceLocator.Get<IBombFactory>();

        // ✅ 하위 시스템 초기화
        var data = DataManager.GetData();
        Movement.Init(data, PlayerSettings.Instance);
        Combat.Init(data, _bombFactory, _eventBus);
        Visuals.Init(data);
        Network.Init(this);
    }

    // ✅ IDamagable 구현 (위임)
    public void TakeDamage(int damage, int attackerViewId)
    {
        Combat.TakeDamage(damage, attackerViewId);
    }

    // ✅ IPlayerHealth 구현 (위임)
    public int Health => CurrentHealth;
    public event Action<int> OnHealthChanged
    {
        add => DataManager.OnHealthChanged += value;
        remove => DataManager.OnHealthChanged -= value;
    }

    // ✅ IPlayerCombat 구현 (위임)
    public void FireBomb(Vector2 direction, EBombType type)
    {
        Combat.FireBomb(direction, type);
    }

    // ✅ IPlayerMovement 구현 (위임)
    public void Move(Vector2 input)
    {
        Movement.Move(input);
    }

    public void Jump()
    {
        Movement.Jump();
    }
}
```

---

### 3.3 참조 복구 전략 (점진적 리팩토링)

**목적:**
- 기존 코드를 한 번에 다 깨뜨리지 않고, **점진적으로** 하위 모듈을 연동합니다.

**전략:**
1. 기존 코드를 보존하면서, 하위 시스템을 추가
2. 기존 메서드에서 하위 시스템으로 위임
3. 기존 메서드를 점진적으로 제거

**✅ Good Case (점진적 리팩토링):**

```csharp
// ✅ 단계 1: 기존 코드 보존하면서 하위 시스템 추가
public class Player : MonoBehaviourPun, IDamagable
{
    // ========== 기존 코드 (보존) ==========
    private PlayerStat _playerStat;
    private PlayerFSM _playerFSM;

    // ========== 새로운 하위 시스템 (추가) ==========
    private PlayerDataManager _dataManager;  // 새로 추가
    private PlayerCombat _combat;          // 새로 추가

    void Awake()
    {
        // 기존 코드 (그대로 유지)
        _playerStat = GetComponent<PlayerStat>();
        _playerFSM = GetComponent<PlayerFSM>();

        // ✅ 새로운 하위 시스템 GetComponent로 연결
        _dataManager = GetComponent<PlayerDataManager>();
        _combat = GetComponent<PlayerCombat>();
    }

    // ✅ 단계 2: 기존 메서드에서 하위 시스템으로 위임 (점진적)
    public void TakeDamage(int damage, int attackerViewId)
    {
        // 기존 코드 (그대로 유지)
        // _playerStat.DecreaseHealth(damage, attackerViewId);

        // ✅ 새로운 하위 시스템으로 위임 (이후 기존 코드 제거)
        _combat.TakeDamage(damage, attackerViewId);
    }

    public void FireBomb(Vector2 direction, EBombType type)
    {
        // 기존 코드 (그대로 유지)
        // FireBombInternal(direction, type);

        // ✅ 새로운 하위 시스템으로 위임
        _combat.FireBomb(direction, type);
    }
}

// ✅ 단계 3: 하위 시스템 구현 (새로 추가)
// 이 코드는 완전히 새로 작성

// ✅ 단계 4: 테스트 및 검증
// - 새로운 하위 시스템 동작 확인
// - 기존 기능 깨짐 확인
// - 네트워크 동기화 검증

// ✅ 단계 5: 기존 코드 제거
// - _playerStat.DecreaseHealth 호출 제거
// - 기존 메서드 제거
```

**리팩토링 체크리스트:**

| 단계 | 작업 | 완료 여부 |
|------|------|-----------|
| 1 | 기존 코드 백업 (Git 커밋) | [ ] |
| 2 | PlayerData 데이터 클래스 작성 | [ ] |
| 3 | PlayerDataManager 작성 (데이터 조작) | [ ] |
| 4 | PlayerMovement 작성 (이동 로직) | [ ] |
| 5 | PlayerCombat 작성 (전투 로직) | [ ] |
| 6 | PlayerNetworkBridge 작성 (네트워크) | [ ] |
| 7 | Player.cs에서 GetComponent로 연결 | [ ] |
| 8 | 기존 메서드에서 하위 시스템으로 위임 | [ ] |
| 9 | 단위 테스트 작성 | [ ] |
| 10 | 통합 테스트 (인게임 플레이) | [ ] |
| 11 | 기존 코드 제거 | [ ] |

---

## 4. 요약

이 문서에서 제시한 핵심 가이드라인:

1. **Singleton 직접 참조 금지** → 의존성 주입 또는 Service Locator 사용
2. **`async void` 금지** → `async Task` 또는 `async UniTask` 사용, 메서드명 `~Async`로 끝내기
3. **데이터-로직 분리** → 데이터 클래스는 순수 데이터, 로직은 별도 클래스, UI는 이벤트만 구독
4. **Player 분해 3단계** → ① 인터페이스 추출 → ② 컴포넌트 분리 → ③ Facade 역할만 남기기

이 규칙을 준수하면, **유지보수 용이하고 테스트 가능한 코드**를 작성할 수 있습니다.

---

**문서 작성일**: 2026년 1월 26일
**버전**: 1.0
**작성자**: Tech Lead & Refactoring Manager
