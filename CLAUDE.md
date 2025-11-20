# GunPowder 프로젝트 분석 문서

> Unity + Photon PUN2 기반 실시간 멀티플레이어 배틀 게임
>
> 분석 일자: 2025-10-29

---

## 목차

1. [프로젝트 개요](#1-프로젝트-개요)
2. [프로젝트 구조](#2-프로젝트-구조)
3. [핵심 시스템 아키텍처](#3-핵심-시스템-아키텍처)
4. [주요 클래스 상세 분석](#4-주요-클래스-상세-분석)
5. [네트워크 동기화 구조](#5-네트워크-동기화-구조)
6. [발견된 문제점](#6-발견된-문제점)
7. [개선 권장 사항](#7-개선-권장-사항)

---

## 1. 프로젝트 개요

### 1.1 기술 스택

- **엔진**: Unity (2D/3D 혼합)
- **네트워크**: Photon PUN2
- **백엔드**: TheBackend SDK
- **채팅**: Backend Chat SDK
- **상태 관리**: RobustFSM (State Pattern)
- **언어**: C# (.NET Standard)

### 1.2 게임 개념

플레이어들이 폭탄을 던져 서로를 공격하는 멀티플레이어 배틀 게임입니다.

**핵심 메커니즘:**
- **건파우더(GunPowder)**: 체력 개념. 공격받으면 감소하고, 일정 시간 공격하지 않으면 자동 감소
- **폭탄 시스템**: 다양한 종류의 폭탄 (일반, 튕기는, 미사일, 박격포, 자폭, 물폭탄)
- **궁극기 시스템**: 위기 상황에서 1회 사용 가능한 강력한 스킬
- **팀 모드**: 팀전 및 개인전 지원

### 1.3 프로젝트 규모

- **총 스크립트 파일**: 371개 (Assets/02.Scripts)
- **주요 씬**: 14개
  - StartSequence (시작)
  - Lobby (로비)
  - Photon (포톤 연결)
  - WaitingRoom (대기실)
  - Beach1, Dock1, Forest1 (맵)
  - Tutorial (튜토리얼)
  - Shop (상점)
  - ResultScene (결과)
  - Credit (크레딧)

---

## 2. 프로젝트 구조

### 2.1 폴더 구조 (Assets/)

```
Assets/
├── 01.Scenes/              # 씬 파일들
├── 02.Scripts/             # 모든 C# 스크립트 (371개 파일)
│   ├── Account/            # 계정 시스템 (DDD 패턴)
│   │   ├── 1. Domain/
│   │   ├── 2. Repository/
│   │   ├── 3. Manager/
│   │   ├── 4. UI/
│   │   └── Specification/
│   ├── Bomb/               # 폭탄 시스템
│   ├── Buff/               # 버프 시스템
│   ├── Camera/             # 카메라 컨트롤
│   ├── Chat/               # 채팅 시스템 (Backend Chat)
│   ├── Common/             # 싱글톤, 유틸리티
│   ├── Emotion/            # 이모션 시스템
│   ├── Explosion/          # 폭발 효과
│   ├── Friend/             # 친구 시스템
│   ├── Gimmick/            # 맵 기믹 (에어드롭, 게, 대포)
│   ├── GunPowder/          # 건파우더 드롭 시스템
│   ├── ItemDatabase/       # 아이템 데이터베이스
│   ├── ItemStorage/        # 인벤토리
│   ├── Map/                # 맵 관리
│   ├── Network/            # 백엔드 통신
│   ├── Party/              # 파티 시스템
│   ├── Photon/             # Photon 네트워크
│   │   ├── Lobby/
│   │   ├── WaitingRoom/
│   │   └── ResultMap/
│   ├── Player/             # 플레이어 시스템
│   │   ├── State/          # FSM 상태들 (21개)
│   │   ├── Skin/
│   │   └── Ultimate/
│   ├── Pool/               # 오브젝트 풀링
│   ├── Shop/               # 상점
│   ├── Sound/              # 사운드 관리
│   ├── Tutorial/           # 튜토리얼
│   └── UI/                 # UI 시스템
├── 03.Prefabs/             # 프리팹
├── 04.ScriptableObject/    # SO 데이터
├── 05.Images/              # 이미지 리소스
├── 06.Models/              # 3D 모델
├── 07.Sounds/              # 사운드 파일
├── 08.Animations/          # 애니메이션
├── BACKND/                 # 뒤끝 SDK
├── Photon/                 # Photon SDK
└── Resources/              # 런타임 로드 리소스
```

### 2.2 코드 구조 특징

1. **DDD 패턴 적용** (Account 시스템)
   - Domain → Repository → Manager → UI 계층 분리
   - Specification 패턴으로 유효성 검증

2. **Singleton 패턴 활용**
   - `PhotonSingleton<T>`: 씬 단위 싱글톤
   - `DontDestroySingleton<T>`: 씬 전환 시 유지
   - `Singleton<T>`: 일반 싱글톤

3. **State Pattern** (플레이어)
   - RobustFSM 라이브러리 사용
   - 21개 상태 관리 (Idle, Walk, Run, Jump, Die 등)

4. **Observer Pattern**
   - EventManager 중앙 집중식 이벤트 관리
   - 26개 이상의 게임 이벤트

5. **Object Pooling**
   - ExplosionPool, VFXPool, InstantiateDestroyManager

---

## 3. 핵심 시스템 아키텍처

### 3.1 Manager 계층

#### GameManager (PhotonSingleton)
**위치**: `Assets/02.Scripts/GameManager.cs`

**책임:**
- 게임 상태 관리 (EGameState)
  - Waiting: 게임 시작 대기
  - Playing: 게임 진행 중
  - GameOver: 게임 종료
  - Result: 결과 표시
  - Tutorial: 튜토리얼 모드
- 플레이어 사망/생존 체크
- 게임 종료 조건 판단 (1명 남음 or 팀전 승리)
- LastPlayer 상태 관리 (2명 남았을 때)

**주요 메서드:**
```csharp
public void CheckGameStatus()           // 게임 상태 체크
public void SetPlayerDead(int viewID)   // 플레이어 사망 처리
public void SetPlayerAlive(int viewID)  // 플레이어 부활 처리
```

---

#### PhotonServerManager (MonoBehaviourPunCallbacks)
**위치**: `Assets/02.Scripts/Photon/PhotonServerManager.cs`

**책임:**
- Photon 서버 연결/로비 관리
- 방 목록 갱신
- 플레이어 입퇴장 콜백 처리
- 데이터 송수신 빈도 설정
  - SendRate: 30 (초당 30회 전송)
  - SerializationRate: 30 (초당 30회 직렬화)

**네트워크 설정:**
```csharp
PhotonNetwork.SendRate = 30;
PhotonNetwork.SerializationRate = 30;
```

---

#### EventManager (DontDestroySingleton)
**위치**: `Assets/02.Scripts/Event/EventManager.cs`

**책임:**
- 게임 전역 이벤트 허브
- 26개 이상의 이벤트 관리

**주요 이벤트:**
```csharp
public event Action<int, int, int> OnDataChanged;        // 체력 변경
public event Action<PhotonPlayer> OnPlayerLeft;          // 플레이어 퇴장
public event Action OnLoadFinished;                      // 로딩 완료
public event Action<int, bool> OnUltimate;               // 궁극기 사용
public event Action<PhotonPlayer> OnPlayerItemChanged;   // 아이템/스킨 변경
public event Action OnGameStart;                         // 게임 시작
public event Action OnGameOver;                          // 게임 종료
```

**사용 패턴:**
```csharp
// 구독
EventManager.Instance.OnDataChanged += HandleDataChanged;

// 발행
EventManager.Instance.OnDataChanged?.Invoke(viewID, currentGP, maxGP);
```

---

### 3.2 네트워크 시스템

#### RoomManager (PhotonSingleton)
**위치**: `Assets/02.Scripts/Photon/WaitingRoom/RoomManager.cs`

**구조:**
```csharp
public class RoomManager : PhotonSingleton<RoomManager>
{
    public RoomInitializer Initializer;     // 방 초기화
    public RoomReadyCheck ReadyCheck;       // 준비 상태 체크
    public RoomPlayerList PlayerList;       // 플레이어 목록 관리
    public PlayerSpawner Spawner;           // 플레이어 스폰

    public EMap SelectedMap;                // 선택된 맵
    public EInGameTeam SelectedTeam;        // 선택된 팀
}
```

**책임:**
- 방 초기화 및 플레이어 배치
- 준비 상태 체크 (모든 플레이어 준비 완료 시 게임 시작)
- 맵/팀 선택 동기화
- CustomProperties 변경 감지
  - IsReady (준비 상태)
  - Team (팀 변경)
  - ItemType (아이템/스킨 변경)

---

#### LobbyManager (PhotonSingleton)
**위치**: `Assets/02.Scripts/Photon/Lobby/LobbyManager.cs`

**책임:**
- 방 생성 로직
- 방 프로퍼티 설정
  - PlayTime: 플레이 시간
  - Life: 라이프 수
  - Gunpowder: 시작 건파우더
  - DeclinePowder: 감소 건파우더 설정
- 비공개방 지원 (비밀번호)

**방 프로퍼티:**
```csharp
public enum ERoomProperties
{
    MapSelected,
    PlayTime,
    Life,
    Gunpowder,
    DeclinePowder,
    IsLocked,
    Password
}
```

---

### 3.3 플레이어 시스템

#### Player (MonoBehaviourPun, IDamagable)
**위치**: `Assets/02.Scripts/Player/Player.cs`
**크기**: 1798 라인 (거대한 클래스)

**주요 책임:**

1. **건파우더 관리**
   - 공격 없을 시 자동 감소 시스템
   - 시각적 경고 (빨간색 점멸, 펄스 효과)
   - 사운드 경고 (점점 빨라지는 경고음)

2. **폭탄 발사**
   - 일반 폭탄 / 특수 폭탄 쿨다운 관리
   - 8방향 폭탄 발사 지점

3. **궁극기 시스템**
   - HasUltimateChance: 사용 가능 상태
   - HasUsedUltimateThisLife: 생애 1회 제한
   - 타이머 기반 기회 지속
   - 파티클 효과 동기화

4. **피격 처리**
   - 팀 체크 (아군 공격 무시)
   - 데미지 팝업 동기화
   - 건파우더 드롭

5. **스킨 시스템**
   - 동적 스킨 적용/해제
   - PlayerSkinManager 위임 패턴

**주요 RPC:**
```csharp
[PunRPC] void RPC_TakeDamage(int damage, int attackerViewId, bool isNormalAttack)
[PunRPC] void RPC_LoadItems()
[PunRPC] void RPC_ChangeState(string stateName)
[PunRPC] void UltimateEffect(bool isOn)
[PunRPC] void RPC_SetMaterial(int materialType)
```

**건파우더 자동 감소 로직:**
```csharp
void Update()
{
    if (!PhotonView.IsMine) return;

    _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;

    if (_gunPowderDecreaseWithoutAttackTimer >= PlayerStat.AttackPenaltyTime)
    {
        _playerStat.DecreaseGunPowderCount(DeclinePowder);
        _gunPowderDecreaseWithoutAttackTimer = 0f;
    }
}
```

---

#### PlayerStat (MonoBehaviour)
**위치**: `Assets/02.Scripts/Player/PlayerStat.cs`

**데이터 중심 클래스**

**주요 데이터:**
```csharp
// 이동 관련
public float MoveSpeed;
public float DashSpeed;
public float JumpPower;

// 건파우더/라이프
private int _currentPlayerGunPowderCount;
private int _currentPlayerLifeCount;
private int _maxPlayerGunpowderCount;
private int _maxPlayerLifeCount;

// 궁극기
private bool _hasUltimateChance;
private bool _hasUsedUltimateThisLife;
private int _ultimateTriggerThreshold;

// 공격자 추적
private int _lastAttacker = -1;
private float _lastAttackTime = -5f;
private const float _lastAttackerTimeout = 5f;

// 통계
private int _totalDamageDealt;
private int _killCount;
```

**핵심 로직:**
```csharp
public bool DecreaseGunPowderCount(int amount, int attacker, bool isNormalAttack = true)
{
    // 공격자 기록 (5초 이내)
    RecordLastAttacker(attacker);

    // 궁극기 기회 발생 조건
    if (_currentPlayerGunPowderCount <= _ultimateTriggerThreshold &&
        !_hasUltimateChance && !_hasUsedUltimateThisLife)
    {
        _hasUltimateChance = true;
        // 이벤트 발행
    }

    // 사망 처리
    if (_currentPlayerGunPowderCount <= 0)
    {
        // 최근 공격자에게 킬 부여
        int killerForLog = GetValidLastAttacker();
        // ...
    }
}

// 최근 공격자 반환 (5초 이내)
private int GetValidLastAttacker()
{
    if (Time.time - _lastAttackTime <= _lastAttackerTimeout)
        return _lastAttacker;
    return -1;
}
```

---

#### PlayerFSM (MonoFSM)
**위치**: `Assets/02.Scripts/Player/State/PlayerFSM.cs`

**RobustFSM 라이브러리 사용**

**등록된 상태 (21개):**
```
- PlayerIdleState          : 대기
- PlayerWalkState          : 걷기
- PlayerRunState           : 달리기
- PlayerJumpState          : 점프
- PlayerDashState          : 대시
- PlayerJumpDashState      : 점프 중 대시
- PlayerDieState           : 사망
- PlayerLastDieState       : 라이프 소진 사망
- PlayerFallDeadState      : 낙사
- PlayerDamagedState       : 피격
- PlayerRecoilState        : 반동
- PlayerHitStopState       : 히트스톱
- PlayerObserveState       : 관전 모드
- PlayerConfuseState       : 혼란 상태
- PlayerCrabHoldedState    : 게에게 붙잡힌 상태
- PlayerShieldState        : 방패 상태
```

**네트워크 동기화:**
```csharp
public void SyncStateChange<T>() where T : PlayerBaseState
{
    if (Owner.PhotonView.IsMine)
    {
        Owner.PhotonView.RPC(nameof(Owner.RPC_ChangeState), RpcTarget.All, typeof(T).Name);
    }
}

[PunRPC]
public void RPC_ChangeState(string stateName)
{
    // 사망 상태에서 특정 상태로만 전환 가능 (가드 조건)
    if (fsmForGuard.IsCurrentState<PlayerDieState>() &&
        stateName != nameof(PlayerIdleState) && ...)
    {
        return;
    }

    // 상태 전환
    playerFSM.ChangeState<T>();
}
```

---

### 3.4 폭탄 시스템

#### Bomb (MonoBehaviourPun, IBomb)
**위치**: `Assets/02.Scripts/Bomb/Bomb.cs`

**구조:**
```csharp
public abstract class Bomb : MonoBehaviourPun, IBomb
{
    protected BombStat _stat;
    protected float _fuzeTimer;

    public int OwnerViewID { get; set; }
    public int Priority { get; set; }

    protected virtual void Update()
    {
        if (!PhotonView.IsMine) return;  // 소유자만 타이머 관리

        _fuzeTimer += Time.deltaTime;
        if (_fuzeTimer >= _stat.FuzeTime)
        {
            PhotonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }

    [PunRPC]
    public virtual void Explode()
    {
        // 폭발 효과 생성
        // ...

        // 오브젝트 파괴 (소유자만)
        if (PhotonView.IsMine)
        {
            if (PhotonView != null && PhotonView.ViewID != 0)
                PhotonNetwork.Destroy(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
```

**상속 클래스:**
- `BasicBomb`: 기본 폭탄
- `BounceBomb`: 튕기는 폭탄
- `MissileBomb`: 미사일 (추적 기능)
- `Mortar`: 박격포 (포물선 궤적)
- `SuicideBomb`: 자폭 폭탄
- `WaterBomb`: 물폭탄 (느려지게 함)

**궁극기 폭탄:**
- `BounceBombUltimate`
- `MissileUltimate`
- `MortarUltimate`
- `SuicideUltimate`
- `WaterBombUltimate`

---

#### Explosion 시스템
**위치**: `Assets/02.Scripts/Explosion/`

**구조:**
```csharp
public class Explosion : MonoBehaviour
{
    public void Initialize(int ownerViewID, int damage)
    {
        // 폭발 반경 내 플레이어 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            IDamagable damagable = hit.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage, ownerViewID);
            }
        }
    }
}
```

**오브젝트 풀링:**
- `ExplosionPool`: 폭발 효과 풀
- 생성/파괴 비용 절감

---

### 3.5 채팅 시스템

#### UIChatManager (Singleton, IChatClientListener)
**위치**: `Assets/02.Scripts/Chat/UIChatManager.cs`

**Backend Chat SDK 사용**

**구조:**
```csharp
public class UIChatManager : Singleton<UIChatManager>, IChatClientListener
{
    private ChatClient _chatClient;

    // 3단계 중첩 딕셔너리
    private Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>> _channelList;

    // 현재 채널 정보
    private string _currentChannelGroup;
    private string _currentChannelName;
    private UInt64 _currentChannelNumber;
}
```

**주요 기능:**
```csharp
// 채팅 메시지 전송
public void SendChatMessage(string text)
{
    // 귓속말 처리
    if (text.IndexOf("/w") == 0)
    {
        string[] whisper = text.Split(' ');
        _chatClient.SendWhisperMessage(whisper[1], message);
    }
    // 번역 기능 (미구현)
    else if (text.IndexOf("/translate") == 0)
    {
        // TODO
    }
    // 일반 채팅
    else
    {
        _chatClient.SendMessage(text);
    }
}

// 채팅 수신
public void OnMessage(Message message)
{
    // 메시지 표시
}
```

**인게임 채팅:**
**위치**: `Assets/02.Scripts/UI/Ingame/UI_IngameChat.cs`

```csharp
public class UI_IngameChat : UI_Popup
{
    private void SendChatMessage()
    {
        if (ChatInput.text.Length == 0) return;

        string text = ChatInput.text;
        ChatInput.text = string.Empty;

        UIChatManager.Instance.SendMessage(text);  // ❌ 버그 발견!
    }
}
```

---

## 4. 주요 클래스 상세 분석

### 4.1 GameManager 상세

**게임 상태 흐름:**
```
Waiting → Playing → GameOver → Result
   ↓
Tutorial (튜토리얼 모드)
```

**게임 종료 조건:**
```csharp
void CheckGameStatus()
{
    int aliveCount = GetAlivePlayerCount();

    if (aliveCount <= 1)
    {
        // 1명 남음 → 게임 종료
        GameOver();
    }
    else if (aliveCount == 2)
    {
        // 2명 남음 → LastPlayer 상태
        SetLastPlayer();
    }
}
```

**팀전 종료 조건:**
- 한 팀만 생존 시 게임 종료
- 팀원 전멸 → 팀 패배

---

### 4.2 RPC 패턴 분석

#### Wrapper 패턴 (권장)
```csharp
// Public 메서드: 소유자 체크 + RPC 호출
public void RPC_UltimateEffect(bool isOn)
{
    if (!PhotonView.IsMine) return;
    PhotonView.RPC(nameof(UltimateEffect), RpcTarget.All, isOn);
}

// PunRPC 메서드: 실제 로직
[PunRPC]
public void UltimateEffect(bool isOn)
{
    // 모든 클라이언트에서 실행
    ultimateParticle.SetActive(isOn);
}
```

#### 직접 호출 패턴
```csharp
[PunRPC]
public void RPC_TakeDamage(int damage, int attackerViewId, bool isNormalAttack)
{
    // 모든 클라이언트에서 실행
    // RPC 내부에서 소유자 체크
    if (PhotonView.IsMine)
    {
        _playerStat.DecreaseGunPowderCount(damage, attackerViewId, isNormalAttack);
    }

    // 공통 로직 (이펙트 등)
    ShowDamageEffect();
}
```

---

### 4.3 CustomProperties 동기화

**Player Properties:**
```csharp
public enum EProperties
{
    IsDead,         // bool
    Kill,           // int
    Damage,         // int
    Team,           // EInGameTeam
    IsReady,        // bool
    CharacterType,  // int
}

// 아이템 Properties
public enum EItemType
{
    Bomb,
    Head,
    Face,
    Chest,
    Cape
}
```

**Room Properties:**
```csharp
public enum ERoomProperties
{
    MapSelected,    // EMap
    PlayTime,       // int
    Life,           // int
    Gunpowder,      // int
    DeclinePowder,  // int
    IsLocked,       // bool
    Password        // string
}
```

**콜백 패턴:**
```csharp
public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
{
    if (changedProps.ContainsKey(EProperties.IsDead.ToString()))
    {
        bool isDead = (bool)changedProps[EProperties.IsDead.ToString()];
        // 사망 처리
    }

    if (changedProps.ContainsKey(EProperties.Team.ToString()))
    {
        EInGameTeam team = (EInGameTeam)changedProps[EProperties.Team.ToString()];
        // 팀 변경 처리
    }
}
```

---

## 5. 네트워크 동기화 구조

### 5.1 네트워크 아키텍처

**Authoritative Pattern:**
- **타이머**: 소유자(Owner) 권한
- **생성/파괴**: 마스터 클라이언트 권한
- **상태 변경**: 소유자 → RPC로 전파

**데이터 플로우:**
```
[Owner Client]
    ↓ (Input/Action)
[Local Change]
    ↓ (RPC)
[All Clients]
    ↓ (Callback)
[EventManager]
    ↓ (Event)
[UI Update]
```

---

### 5.2 오브젝트 생성 패턴

#### PhotonNetwork.Instantiate
```csharp
// GunPowder 생성 (마스터 클라이언트만)
if (!PhotonNetwork.IsMasterClient) return;

object[] instData = new object[]
{
    attackerViewId,
    isFallingOut,
    randomSeed,
    PhotonView.ViewID
};

PhotonNetwork.Instantiate(
    GunPowderPrefab.name,
    spawnPos,
    Quaternion.identity,
    0,
    instData
);
```

#### 오브젝트 풀링
```csharp
// Explosion (로컬 생성)
Explosion explosion = ExplosionPool.Instance.Get();
explosion.Initialize(ownerViewID, damage);
```

---

### 5.3 RPC 호출 현황

**총 104개 파일**에서 Photon 사용 (PhotonView, PunRPC 포함)

**주요 RPC 메서드:**

**Player:**
- `RPC_TakeDamage`: 피격 처리
- `RPC_ChangeState`: 상태 변경
- `RPC_LoadItems`: 아이템 로드
- `RPC_ChangeGunpowder`: 건파우더 변경
- `RPC_RequestIncreaseGunPowder`: 건파우더 증가 요청
- `UltimateEffect`: 궁극기 이펙트

**Bomb:**
- `Explode`: 폭발

**GameManager:**
- `RPC_GameStart`: 게임 시작
- `RPC_GameOver`: 게임 종료

---

## 6. 발견된 문제점

### 6.1 🔴 Critical: Chat 시스템 버그

**파일**: `Assets/02.Scripts/UI/Ingame/UI_IngameChat.cs:42`

**문제:**
```csharp
UIChatManager.Instance.SendMessage(text);  // ❌ 메서드가 존재하지 않음
```

**실제 메서드 (UIChatManager.cs:64):**
```csharp
public void SendChatMessage(string text)
```

**해결 방안:**
```csharp
// UI_IngameChat.cs:42 수정
UIChatManager.Instance.SendChatMessage(text);
```

**영향:**
- 인게임 채팅 전송 불가
- 컴파일 에러는 아니지만 런타임 오류 발생 가능 (MonoBehaviour.SendMessage로 호출될 수 있음)

---

### 6.2 🟡 Medium: Player 클래스 과도한 책임

**파일**: `Assets/02.Scripts/Player/Player.cs`
**크기**: 1798 라인

**문제점:**
- 단일 책임 원칙(SRP) 위반
- 건파우더, 폭탄, 궁극기, 스킨, 피격 처리 모두 포함
- 유지보수 어려움
- 테스트 어려움

**권장 리팩토링:**
```
Player.cs (핵심 로직만)
├── PlayerCombat.cs         (폭탄 발사, 피격 처리)
├── PlayerGunpowderManager.cs (건파우더 관리)
├── PlayerUltimate.cs       (궁극기 시스템)
└── PlayerVisuals.cs        (스킨, 이펙트, 사운드)
```

**장점:**
- 각 컴포넌트 독립 테스트 가능
- 코드 가독성 향상
- 협업 시 충돌 감소

---

### 6.3 🟡 Medium: 타이밍 동기화 이슈

#### 건파우더 자동 감소
**파일**: `Assets/02.Scripts/Player/Player.cs`

**현재 구현:**
```csharp
void Update()
{
    if (!PhotonView.IsMine) return;

    _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;

    if (_gunPowderDecreaseWithoutAttackTimer >= PlayerStat.AttackPenaltyTime)
    {
        _playerStat.DecreaseGunPowderCount(DeclinePowder);
        _gunPowderDecreaseWithoutAttackTimer = 0f;
    }
}
```

**문제:**
- 각 클라이언트에서 독립적으로 타이머 실행
- 네트워크 지연 시 동기화 오차 발생 가능
- 공격 타이밍에 따라 다른 결과 가능

**해결 방안:**
```csharp
// PhotonNetwork.Time 사용
private double _lastAttackTime;

void Update()
{
    if (!PhotonView.IsMine) return;

    double timeSinceLastAttack = PhotonNetwork.Time - _lastAttackTime;

    if (timeSinceLastAttack >= PlayerStat.AttackPenaltyTime)
    {
        _playerStat.DecreaseGunPowderCount(DeclinePowder);
        _lastAttackTime = PhotonNetwork.Time;
    }
}

void OnAttack()
{
    _lastAttackTime = PhotonNetwork.Time;
}
```

---

### 6.4 🟡 Medium: 팀 동기화 타이밍 이슈

**파일**: `Assets/02.Scripts/Player/Player.cs`

**문제:**
```csharp
// Start()에서 CustomProperties 읽기
if (PhotonView.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()))
{
    _playerStat.Team = (EInGameTeam)PhotonView.Owner.CustomProperties[EProperties.Team.ToString()];
}

// RPC_TakeDamage에서 로컬 필드 사용
EInGameTeam victimTeam = _playerStat.Team;

if (attackerTeam == victimTeam && attackerActorNumber != PhotonView.OwnerActorNr)
{
    return;  // 아군 공격 무시
}
```

**문제점:**
- CustomProperties와 로컬 필드(`_playerStat.Team`) 동기화 타이밍
- 팀 변경 시 즉시 반영 안 될 수 있음

**해결 방안:**
```csharp
// 항상 CustomProperties를 직접 참조
EInGameTeam GetPlayerTeam(PhotonView view)
{
    if (view.Owner == null) return EInGameTeam.Default;

    if (view.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()))
    {
        return (EInGameTeam)view.Owner.CustomProperties[EProperties.Team.ToString()];
    }

    return EInGameTeam.Default;
}

// 사용
EInGameTeam victimTeam = GetPlayerTeam(PhotonView);
```

**또는:**
```csharp
// OnPlayerPropertiesUpdate에서 즉시 반영
public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
{
    if (targetPlayer != PhotonView.Owner) return;

    if (changedProps.ContainsKey(EProperties.Team.ToString()))
    {
        _playerStat.Team = (EInGameTeam)changedProps[EProperties.Team.ToString()];
    }
}
```

---

### 6.5 🟡 Medium: RPC 호출 순서 및 중복 호출

**파일**: `Assets/02.Scripts/Player/State/PlayerFSM.cs`

**현재 구현:**
```csharp
public void SyncStateChange<T>() where T : PlayerBaseState
{
    if (Owner.PhotonView.IsMine)
    {
        // RpcTarget.All: 자신 포함 모든 클라이언트
        Owner.PhotonView.RPC(nameof(Owner.RPC_ChangeState), RpcTarget.All, typeof(T).Name);
    }
}
```

**잠재적 문제:**
- `RpcTarget.All` 사용 시 자신도 포함
- 로컬에서 먼저 상태 변경 후 RPC 호출 시 중복 호출 가능

**확인:**
```csharp
[PunRPC]
public void RPC_ChangeState(string stateName)
{
    // ✅ 가드 조건 존재
    if (fsmForGuard.IsCurrentState<PlayerDieState>() &&
        stateName != nameof(PlayerIdleState))
    {
        return;
    }

    playerFSM.ChangeState<T>();
}
```

**권장 사항:**
- 현재는 가드 조건으로 안전
- 하지만 명확한 의도 표현을 위해 다음 패턴 고려:
```csharp
public void SyncStateChange<T>() where T : PlayerBaseState
{
    // 로컬 먼저 변경
    playerFSM.ChangeState<T>();

    // 다른 클라이언트에게만 전파
    Owner.PhotonView.RPC(nameof(Owner.RPC_ChangeState), RpcTarget.Others, typeof(T).Name);
}
```

---

### 6.6 🟡 Medium: PhotonView 유효성 체크 누락 가능성

**현재 안전한 코드 (Bomb.cs):**
```csharp
[PunRPC]
public virtual void Explode()
{
    // ...
    if (PhotonView.IsMine)
    {
        if (PhotonView != null && PhotonView.ViewID != 0)  // ✅ 안전장치
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);  // 로컬 파괴
        }
    }
}
```

**권장 사항:**
- 모든 `PhotonNetwork.Destroy` 호출 전 유효성 체크
- 전역 검토 필요

**체크리스트:**
```csharp
// PhotonNetwork.Destroy 전에 항상 체크
if (PhotonView != null && PhotonView.ViewID != 0)
{
    PhotonNetwork.Destroy(gameObject);
}
else
{
    Destroy(gameObject);
}
```

---

### 6.7 🟢 Low: 메모리 누수 가능성

**파일**: `Assets/02.Scripts/Player/Player.cs`

**현재 OnDestroy:**
```csharp
private void OnDestroy()
{
    if (EventManager.Instance != null)
    {
        EventManager.Instance.OnPlayerItemChanged -= LoadItems;
    }
    if (_playerStat != null)
    {
        _playerStat.OnGunPowderEmpty -= HandleGunpowderEmpty;
        _playerStat.OnGunpowderIncreased -= HandleGunpowderIncreased;
    }
}
```

**확인 필요:**
- 다른 이벤트 구독 해제 누락 가능성
- DOTween 시퀀스 정리 (`_preExplosionPulseTween`)

**권장 추가:**
```csharp
private void OnDestroy()
{
    // 기존 코드...

    // DOTween 시퀀스 정리
    _preExplosionPulseTween?.Kill();
    _preExplosionPulseTween = null;

    // 코루틴 정지
    StopAllCoroutines();
}
```

---

### 6.8 🟢 Low: 건파우더 중복 획득 가능성

**파일**: `Assets/02.Scripts/GunPowder/GunPowder.cs`

**현재 구현:**
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    Player player = collision.GetComponent<Player>();
    if (player != null && player.PhotonView.IsMine)
    {
        // 건파우더 증가 RPC 호출
        player.PhotonView.RPC("RPC_RequestIncreaseGunPowder", RpcTarget.All, amount);

        // 마스터 클라이언트만 파괴
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
```

**문제 가능성:**
- 여러 플레이어가 동시 획득 시 중복 가능성?
- **확인 결과**: 마스터 클라이언트만 생성/파괴하므로 안전

**현재 상태**: ✅ 올바르게 구현됨

---

## 7. 개선 권장 사항

### 7.1 즉시 수정 필요 (High Priority)

#### 1. UI_IngameChat.cs 버그 수정
**위치**: `Assets/02.Scripts/UI/Ingame/UI_IngameChat.cs:42`

```csharp
// 수정 전
UIChatManager.Instance.SendMessage(text);

// 수정 후
UIChatManager.Instance.SendChatMessage(text);
```

#### 2. PhotonView 유효성 체크 전역 검토
**대상**: 모든 `PhotonNetwork.Destroy` 호출

```csharp
// 패턴 적용
if (PhotonView != null && PhotonView.ViewID != 0)
    PhotonNetwork.Destroy(gameObject);
else
    Destroy(gameObject);
```

#### 3. 이벤트 구독 해제 전역 검토
**대상**: 모든 MonoBehaviour의 `OnDestroy`

**체크리스트:**
- EventManager 이벤트 구독 해제
- DOTween 시퀀스 정리
- 코루틴 정지

---

### 7.2 중기 개선 사항 (Medium Priority)

#### 1. Player 클래스 리팩토링

**현재**: 1798 라인의 거대한 클래스

**리팩토링 계획:**

**Player.cs** (핵심만 유지)
```csharp
public class Player : MonoBehaviourPun, IDamagable
{
    // References
    public PlayerStat Stat;
    public PlayerFSM FSM;
    public PlayerCombat Combat;
    public PlayerGunpowderManager GunpowderManager;
    public PlayerUltimate Ultimate;
    public PlayerVisuals Visuals;

    // Initialization
    void Start() { }

    // IDamagable
    public void TakeDamage(int damage, int attackerViewId)
    {
        Combat.TakeDamage(damage, attackerViewId);
    }
}
```

**PlayerCombat.cs** (새 파일)
```csharp
public class PlayerCombat : MonoBehaviour
{
    private Player _owner;

    // 폭탄 발사
    public void FireBomb(Vector2 direction) { }

    // 피격 처리
    public void TakeDamage(int damage, int attackerViewId) { }

    // 팀 체크
    private bool IsAlly(int attackerViewId) { }
}
```

**PlayerGunpowderManager.cs** (새 파일)
```csharp
public class PlayerGunpowderManager : MonoBehaviour
{
    private Player _owner;

    // 자동 감소 타이머
    private double _lastAttackTime;

    void Update()
    {
        CheckAutoDecrease();
    }

    private void CheckAutoDecrease() { }

    public void OnAttack()
    {
        _lastAttackTime = PhotonNetwork.Time;
    }
}
```

**PlayerUltimate.cs** (새 파일)
```csharp
public class PlayerUltimate : MonoBehaviour
{
    private Player _owner;

    public void TryActivateUltimate() { }

    public void OnUltimateChance() { }

    public void OnUltimateUsed() { }
}
```

**PlayerVisuals.cs** (새 파일)
```csharp
public class PlayerVisuals : MonoBehaviour
{
    private Player _owner;

    // 스킨 관리
    public void LoadSkin(int skinId) { }

    // 이펙트
    public void ShowDamageEffect() { }
    public void ShowUltimateEffect(bool isOn) { }

    // 사운드
    public void PlaySound(string soundName) { }
}
```

---

#### 2. 타이밍 동기화 개선

**대상**: 건파우더 자동 감소, 궁극기 타이머

**개선 방안:**
```csharp
// PhotonNetwork.Time 사용
private double _lastEventTime;

void CheckEvent()
{
    double currentTime = PhotonNetwork.Time;
    double timeSinceLastEvent = currentTime - _lastEventTime;

    if (timeSinceLastEvent >= threshold)
    {
        TriggerEvent();
        _lastEventTime = currentTime;
    }
}
```

---

#### 3. 팀 동기화 강화

**현재 문제**: CustomProperties와 로컬 필드 불일치 가능

**개선 방안 1: 항상 CustomProperties 참조**
```csharp
public EInGameTeam GetPlayerTeam()
{
    if (PhotonView.Owner == null) return EInGameTeam.Default;

    if (PhotonView.Owner.CustomProperties.ContainsKey(EProperties.Team.ToString()))
    {
        return (EInGameTeam)PhotonView.Owner.CustomProperties[EProperties.Team.ToString()];
    }

    return EInGameTeam.Default;
}
```

**개선 방안 2: 콜백으로 즉시 동기화**
```csharp
public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
{
    if (targetPlayer != PhotonView.Owner) return;

    if (changedProps.ContainsKey(EProperties.Team.ToString()))
    {
        _playerStat.Team = (EInGameTeam)changedProps[EProperties.Team.ToString()];
        Debug.Log($"Team changed to: {_playerStat.Team}");
    }
}
```

---

#### 4. RPC 최적화

**RpcTarget 검토:**
```csharp
// 불필요한 RpcTarget.All 사용 줄이기

// 수정 전
PhotonView.RPC("SomeMethod", RpcTarget.All);

// 수정 후 (본인은 이미 처리했다면)
PhotonView.RPC("SomeMethod", RpcTarget.Others);
```

**데이터 압축:**
```csharp
// bool 배열을 byte로 압축
bool[] flags = new bool[8];
byte compressedFlags = CompressBools(flags);

[PunRPC]
void RPC_UpdateFlags(byte flags)
{
    bool[] decompressed = DecompressBools(flags);
}
```

---

### 7.3 장기 최적화 사항 (Low Priority)

#### 1. 코드 구조 정리

**중복 코드 제거:**
- 공통 RPC 패턴을 베이스 클래스로 추출
- 유틸리티 함수 분리 (PhotonUtil, MathUtil 등)

**네이밍 일관성:**
- RPC 메서드 네이밍 규칙 통일
  - `RPC_MethodName` vs `MethodNameRPC`
  - 현재: 혼재 사용 중

---

#### 2. 문서화 개선

**네트워크 플로우 다이어그램 작성:**
```
[Player Input]
    ↓
[Local Validation]
    ↓
[RPC Call] ────────→ [All Clients]
                         ↓
                    [Apply Change]
                         ↓
                    [EventManager]
                         ↓
                    [UI Update]
```

**RPC 호출 맵:**
- 각 기능별 RPC 호출 순서 문서화
- 데이터 플로우 시각화

---

#### 3. 성능 최적화

**오브젝트 풀링 확대:**
- 현재: Explosion, VFX
- 확대 대상: Bomb, GunPowder, DamagePopup

**SendRate 조정:**
```csharp
// 현재
PhotonNetwork.SendRate = 30;
PhotonNetwork.SerializationRate = 30;

// 최적화 (필요 시)
PhotonNetwork.SendRate = 20;
PhotonNetwork.SerializationRate = 20;
```

---

## 8. 동기화 문제 개선 체크리스트

### 8.1 현재 확인된 동기화 이슈

✅ **해결됨:**
- 폭탄 퓨즈 타이머 (소유자만 관리)
- 건파우더 드롭 (마스터 클라이언트 권한)

⚠️ **주의 필요:**
- 건파우더 자동 감소 타이밍 (PhotonNetwork.Time 사용 권장)
- 팀 변경 시 즉시 반영 (콜백 강화 필요)
- 상태 변경 RPC 중복 호출 (현재는 가드 조건으로 안전)

🔴 **수정 필요:**
- 채팅 시스템 버그 (UI_IngameChat.cs:42)

---

### 8.2 동기화 개선 우선순위

**High Priority (즉시):**
1. UI_IngameChat.cs 버그 수정
2. PhotonView 유효성 체크 전역 검토

**Medium Priority (1-2주 내):**
1. 건파우더 자동 감소 타이밍 개선 (PhotonNetwork.Time)
2. 팀 동기화 강화 (콜백 즉시 반영)
3. Player 클래스 리팩토링

**Low Priority (향후):**
1. RPC 최적화
2. 성능 최적화
3. 문서화

---

### 8.3 권장 테스트 시나리오

**동기화 테스트:**
1. **네트워크 지연 시뮬레이션**
   - Photon Inspector에서 지연 추가 (100ms, 200ms)
   - 건파우더 감소 타이밍 일치 확인

2. **팀 변경 테스트**
   - 게임 중 팀 변경
   - 아군 공격 무시 확인

3. **동시 이벤트 테스트**
   - 여러 플레이어 동시 공격
   - 건파우더 드롭 동시 획득

4. **채팅 테스트**
   - 인게임 채팅 전송
   - 귓속말 기능 확인

---

## 9. 결론

### 9.1 프로젝트 강점

1. **명확한 아키텍처**
   - 싱글톤 패턴 일관성
   - 이벤트 기반 시스템
   - 상태 패턴 활용

2. **네트워크 구조**
   - Authoritative 패턴 올바르게 적용
   - RPC 사용 패턴 일관성
   - CustomProperties 활용

3. **코드 조직**
   - 폴더 구조 체계적
   - DDD 패턴 일부 적용 (Account)

---

### 9.2 개선 필요 영역

1. **채팅 시스템 버그** (Critical)
   - 즉시 수정 필요

2. **Player 클래스 리팩토링** (Medium)
   - 단일 책임 원칙 적용

3. **타이밍 동기화** (Medium)
   - PhotonNetwork.Time 활용

4. **팀 동기화 강화** (Medium)
   - 콜백 즉시 반영

---

### 9.3 최종 권장 사항

**즉시 조치:**
1. UI_IngameChat.cs 버그 수정
2. PhotonView 유효성 체크 전역 검토
3. 이벤트 구독 해제 검토

**단기 개선 (1-2주):**
1. 건파우더 타이밍 동기화 개선
2. 팀 동기화 강화

**중기 개선 (1-2개월):**
1. Player 클래스 리팩토링
2. RPC 최적화

**장기 개선 (향후):**
1. 코드 구조 정리
2. 성능 최적화
3. 문서화 강화

---

## 10. 참고 자료

### 10.1 주요 파일 경로

**핵심 Manager:**
- `Assets/02.Scripts/GameManager.cs`
- `Assets/02.Scripts/Photon/PhotonServerManager.cs`
- `Assets/02.Scripts/Event/EventManager.cs`

**Player 시스템:**
- `Assets/02.Scripts/Player/Player.cs`
- `Assets/02.Scripts/Player/PlayerStat.cs`
- `Assets/02.Scripts/Player/State/PlayerFSM.cs`

**Network:**
- `Assets/02.Scripts/Photon/WaitingRoom/RoomManager.cs`
- `Assets/02.Scripts/Photon/Lobby/LobbyManager.cs`

**Chat (버그 발견):**
- `Assets/02.Scripts/Chat/UIChatManager.cs`
- `Assets/02.Scripts/UI/Ingame/UI_IngameChat.cs` ⚠️

**Bomb:**
- `Assets/02.Scripts/Bomb/Bomb.cs`

---

### 10.2 아키텍처 다이어그램

**Manager 계층:**
```
DontDestroySingleton (씬 전환 시 유지)
├── EventManager
├── ClientManager
├── AccountManager
└── SoundManager

PhotonSingleton (씬 단위)
├── GameManager
├── PhotonServerManager
├── RoomManager
└── LobbyManager
```

**Player 시스템:**
```
Player (MonoBehaviourPun)
├── PlayerStat (데이터)
├── PlayerFSM (상태)
├── PlayerInputController (입력)
├── PlayerSkinManager (스킨)
└── PlayerBuffHandler (버프)
```

**네트워크 플로우:**
```
[Client A]                [Server]                [Client B]
    |                        |                        |
    |──── Fire Bomb ────────>|                        |
    |                        |                        |
    |<──── RPC: Spawn ───────|──── RPC: Spawn ──────>|
    |                        |                        |
    |                    [Bomb Created]           [Bomb Created]
    |                        |                        |
    |──── Explode ──────────>|                        |
    |                        |                        |
    |<──── RPC: Explode ─────|──── RPC: Explode ────>|
```

---

**문서 작성일**: 2025-10-29
**작성자**: Claude Code
**버전**: 1.0
