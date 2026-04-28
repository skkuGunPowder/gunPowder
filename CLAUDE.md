# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity + Photon PUN2 실시간 멀티플레이어 배틀 게임. 플레이어들이 폭탄을 던져 서로를 공격하며, 건파우더(체력 개념)가 소진되면 탈락한다.

**Tech Stack:** Unity (2D/3D 혼합), Photon PUN2, TheBackend SDK, Backend Chat SDK, RobustFSM, DOTween

## Build & Development

이 프로젝트는 Unity Editor에서만 빌드/실행된다. CLI 빌드 명령은 없다.

- **열기:** Unity Hub에서 프로젝트 루트(`gunPowder/`)를 열기
- **멀티플레이 테스트:** ParrelSync 패키지 사용 (Clone Manager로 여러 Editor 인스턴스 동시 실행)
- **Hot Reload:** `com.singularitygroup.hotreload` 패키지 설치되어 있음 — 플레이 모드 중 코드 변경 반영

## Architecture

### Singleton 계층

```
DontDestroySingleton<T>  — 씬 전환에도 유지 (EventManager, AccountManager, SoundManager, UIChatManager)
PhotonSingleton<T>       — 씬 단위 싱글톤 (GameManager, LobbyManager, RoomManager)
Singleton<T>             — 일반 싱글톤
```

`PhotonServerManager`는 예외적으로 수동 싱글톤(`Instance` 필드 + `DontDestroyOnLoad`) 사용.

### 씬 흐름

```
Photon → StartSequence → Lobby → WaitingRoom → [Beach1 / Dock1 / Forest1 / ...] → ResultScene
```

씬 전환은 `PhotonNetwork.AutomaticallySyncScene = true` 로 마스터 클라이언트가 제어한다.

### 네트워크 패턴

**권한 구조:**
- 폭탄 퓨즈 타이머, 상태 전환: **소유자(IsMine)** 권한
- 오브젝트 생성/파괴: **마스터 클라이언트** 권한
- 데이터 동기화: `CustomProperties` (플레이어/룸) + RPC

**표준 RPC 패턴:**
```csharp
// Public wrapper: IsMine 체크 후 RPC 호출
public void DoSomething() {
    if (!PhotonView.IsMine) return;
    PhotonView.RPC(nameof(RPC_DoSomething), RpcTarget.All, args);
}

// [PunRPC]: 모든 클라이언트에서 실행
[PunRPC]
void RPC_DoSomething(args) { /* 실제 로직 */ }
```

**PhotonNetwork.Destroy 호출 전 항상 유효성 체크:**
```csharp
if (PhotonView != null && PhotonView.ViewID != 0)
    PhotonNetwork.Destroy(gameObject);
else
    Destroy(gameObject);
```

### 핵심 열거형 (Assets/02.Scripts/Enum/)

- `EProperties` — 플레이어 CustomProperties 키 (IsReady, IsDead, Kill, Damage, Team, Emotion 등)
- `ERoomProperties` — 룸 CustomProperties 키 (PlayTime, Life, Gunpowder, MapSelected, GameMode, ChatChannel* 등)
- `EItemType` — 아이템 종류 (Head, Face, Chest, Cape, Bomb, BombSkin, BombVFX, BonusCard 등)
- `EInGameTeam` — 팀 구분
- `EGameMode` — Deathmatch 등 게임 모드
- `EModeState` — 룸 상태 변경 동기화용

### Player 시스템 (Assets/02.Scripts/Player/)

`Player.cs`를 중심으로 컴포넌트 분리 구조:
- `PlayerStat` — 수치 데이터 (건파우더, 라이프, 스피드 등)
- `PlayerGunpowderController` — 건파우더 자동 감소 타이머 관리
- `PlayerDamageController` — 피격 처리, 팀 체크
- `PlayerUltimateController` — 궁극기 상태 및 이펙트
- `PlayerVisualController` — 스킨, 이펙트, 사운드
- `PlayerFSM` (Assets/02.Scripts/Player/State/) — RobustFSM 기반 21개 상태 관리

FSM 상태 동기화: `SyncStateChange<T>()` → `RPC_ChangeState(string stateName)` (RpcTarget.All)

### ItemStorage 시스템 (Assets/02.Scripts/ItemStorage/)

DDD 패턴 적용:
```
1.Domain/    — InventoryItem
2.Repository/ — ItemStorageRepo (TheBackend 통신)
3.Manager/   — ItemStorage
4.UI/        — UI_ItemStorage, UI_ItemSlot, UI_Category 등
```

### 이벤트 시스템

`EventManager` (DontDestroySingleton)가 중앙 이벤트 허브 역할. 구독/해제는 반드시 `OnDestroy`에서 처리할 것.

### UI 구조

- `UI_Popup` 베이스 클래스 → `PopupManager`로 관리
- 인게임 채팅: `UIChatManager.Instance.SendChatMessage(text)` (주의: `SendMessage`가 아님)

### 오브젝트 풀링

- `ExplosionPool`, `VFXPool` — 폭발/VFX 효과
- `InstantiateDestroyManager` — 일반 오브젝트 풀

### 타입 선언

`var` 키워드를 사용하지 말고 타입을 명시적으로 선언할 것:

```csharp
// 금지
var list = new List<string>();
var dict = new Dictionary<string, int>();

// 사용
List<string> list = new List<string>();
Dictionary<string, int> dict = new Dictionary<string, int>();
```

### 캐스팅 패턴

`as` + `?.` 조합 대신 `is` 패턴 매칭으로 명시적으로 작성할 것:

```csharp
// 금지
(_gameMode as BattleMode)?.DeathOrderList.Clear();

// 사용
if (_gameMode is BattleMode battleMode)
{
    battleMode.DeathOrderList.Clear();
}
```

## 주요 알려진 이슈

- `UI_IngameChat.cs:42` — `UIChatManager.Instance.SendMessage(text)` 는 잘못된 호출. `SendChatMessage(text)` 를 사용해야 함
- `EProperties`에 `CharacterType` 키가 없음 — CLAUDE.md 분석 문서는 오래된 열거형 기준으로 작성됨. 실제 열거형은 코드에서 확인할 것
