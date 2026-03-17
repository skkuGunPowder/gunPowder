# [KKH] HP/GP 분리 작업내역서

> 작업 브랜치: `KKH_Test`
> 커밋: `6c7098f07` — [KKH] fix. GP 기획 내용 대로 수정, HP 추가
> 작성일: 2026-03-17

---

## 1. 변경점 요약

### 개요

기존에 GP(GunPowder)가 **체력 + 재화**를 겸하던 구조를 분리하였습니다.

| 구분 | 이전 | 현재 |
|------|------|------|
| **체력** | GP가 체력 역할 (로비 설정값) | **HP** 고정 150 |
| **재화** | GP가 재화 역할 (체력과 동일) | **GP** 재화 전용 (음수 허용, 사망 시 유지) |
| **궁극기 발동** | GP ≤ 30 | HP ≤ 30 |
| **사망 조건** | GP ≤ 0 | HP ≤ 0 |
| **리스폰** | GP = 로비 설정값으로 초기화 | 캐리오버 로직 적용 (아래 참조) |

### 리스폰 캐리오버 규칙

| 조건 | HP | GP |
|------|----|----|
| GP ≥ 0 | 150 (초기화) | GP + 50 |
| GP < 0 | 150 - min(\|GP\|, 100) | 50 |

---

### 1-1. PlayerStat.cs — 필드/프로퍼티 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| `_currentPlayerGunPowderCount` | `_currentHP` | 체력 필드 |
| `_initGunpowderCount` (로비 설정값) | `INIT_HP = 150` (const) | 고정값 변경 |
| `CurrentPlayerGunPowderCount` | `CurrentHP` | 프로퍼티 (하위호환 shim 유지) |
| `InitGunpowderCount` | `InitHP` | 프로퍼티 (하위호환 shim 유지) |
| — | `_currentGP` | **신규** GP 재화 필드 |
| — | `CurrentGP` | **신규** GP 프로퍼티 |
| `OnGunPowderEmpty` | `OnHPEmpty` | 이벤트 |
| `OnGunpowderIncreased` | `OnHPIncreased` | 이벤트 |
| `OnGunPowderChanged` | `OnHPChanged` | 이벤트 |
| — | `OnGPChanged` | **신규** GP 변경 이벤트 |

### 1-2. PlayerStat.cs — 메서드 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| `DecreaseGunPowderCount(amount, attacker, ...)` | `DecreaseHP(amount, attacker, ...)` | 하위호환 shim 유지 |
| `IncreaseGunPowderCount(amount)` | `IncreaseHP(amount)` | 하위호환 shim 유지 |
| `RPC_ChangeGunpowder(gunpowder, life, attacker)` | `RPC_ChangeHP(hp, life, attacker)` | 하위호환 shim 유지 |
| `RPC_RequestIncreaseGunPowder(amount)` | `RPC_RequestIncreaseGP(amount)` | 하위호환 shim 유지 |
| `SetPlayerGunPowderCountAndLife(gunpowder, life)` | `SetPlayerHPAndLife(hp, life)` | 하위호환 shim 유지 |
| — | `DecreaseGP(amount)` | **신규** GP 감소 (음수 허용) |
| — | `IncreaseGP(amount)` | **신규** GP 증가 |
| — | `RPC_ChangeGP(gp)` | **신규** GP 네트워크 동기화 |
| — | `RPC_RequestIncreaseGP(amount)` | **신규** GP 픽업 시 호출 |
| — | `ApplyResurrectCarryover()` | **신규** 리스폰 캐리오버 로직 |
| `ResurrectPlayerStat()` — GP를 초기값으로 리셋 | `ResurrectPlayerStat()` — HP는 캐리오버에서 이미 설정, 상태만 초기화 | 로직 변경 |

### 1-3. PlayerDamageController.cs — 피격 로직 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| `CeilToInt(maxDamage * stealPercent)` | `FloorToInt(maxDamage * stealPercent)` | GP 낙출량 계산 변경 |
| `DecreaseGunPowderCount(damage, ...)` | `DecreaseHP(damage, ...)` | HP 감소 |
| — | `DecreaseGP(gpDrop)` | **신규** GP 감소 (피격 시 동시) |
| 변수명 `gunPowderCount` | `gpDrop` | 의미 명확화 |

### 1-4. PlayerGunpowderController.cs — 미공격 페널티 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| `DecreaseGunPowderCount(AttackPenaltyAmount, ...)` | `DecreaseHP(AttackPenaltyAmount, ...)` | HP 감소 |
| — | `DecreaseGP(NO_ATTACK_RELEASE_COUNT)` | **신규** GP 감소 추가 |

### 1-5. EventManager.cs — 이벤트 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| `OnDataChanged` 파라미터: `gunpowder` | `OnDataChanged` 파라미터: `hp` | 의미 변경 |
| `PlayerDataChange(gunpowder, life, ...)` | `PlayerDataChange(hp, life, ...)` | 파라미터명 변경 |
| — | `OnGPDataChanged` (Action\<int, int\>) | **신규** (playerNumber, gp) |
| — | `PlayerGPChange(playerNumber, gp)` | **신규** GP 이벤트 발행 |

### 1-6. DamageChecker.cs — 점수 계산 변경

| 이전 | 현재 | 비고 |
|------|------|------|
| 초기 점수: `PlayerLife * PlayerGunpowder` | 초기 점수: `PlayerLife * 150` | HP 고정값 기준 |
| 점수 계산: `(life * PlayerGunpowder) + gunpowder` | 점수 계산: `(life * 150) + hp` | HP 고정값 기준 |
| `SetPlayerGunPowderCountAndLife(gunpowder, life)` | `SetPlayerHPAndLife(hp, life)` | 메서드명 변경 |

### 1-7. RoomStatManager.cs

| 이전 | 현재 | 비고 |
|------|------|------|
| — | `public const int PlayerHP = 150` | **신규** HP 고정 상수 |
| `PlayerGunpowder` | `PlayerGunpowder` (유지) | GP 초기값 용도로 계속 사용 |

### 1-8. UI 관련 변경

| 파일 | 이전 | 현재 |
|------|------|------|
| **UI_InGameProfileSlot.cs** | `GunpowderTextUGUI` (TMP 1개) | `HPTextUGUI` + `GPTextUGUI` (TMP 2개) |
| 〃 | `GunpowderMiddle = 50` | `HPMiddle = 75` |
| 〃 | `GunpowderLow = 20` | `HPLow = 30` |
| 〃 | `Init(..., int gunpowder, int life)` | `Init(..., int hp, int life, int gp)` |
| 〃 | `Refresh(int gunpowder, ...)` | `Refresh(int hp, ...)` |
| 〃 | — | `RefreshGP(int gp)` **신규** |
| **UI_InGameProfile.cs** | `Init` — GP값 전달 | `Init` — HP(150) + GP 전달 |
| 〃 | — | `RefreshGP(int playerNumber, int gp)` **신규** |
| 〃 | — | `OnGPDataChanged` 구독/해제 **신규** |
| **PlayerHealthBar.cs** | `OnGunPowderChanged` 구독 | `OnHPChanged` 구독 |
| 〃 | `InitGunpowderCount` | `InitHP` |
| 〃 | `CurrentPlayerGunPowderCount` | `CurrentHP` |
| **UI_GunPowderStatus.cs** | `OnGunPowderChanged` 구독 | `OnHPChanged` 구독 |
| 〃 | `CurrentPlayerGunPowderCount` | `CurrentHP` |

### 1-9. Player/State 관련 변경

| 파일 | 이전 | 현재 |
|------|------|------|
| **Player.cs** | `OnGunPowderEmpty += HandleGunPowderEmpty` | `OnHPEmpty += HandleHPEmpty` |
| 〃 | `OnGunpowderIncreased += HandleGunpowderIncreased` | `OnHPIncreased += HandleHPIncreased` |
| **PlayerUltimateController.cs** | `CurrentPlayerGunPowderCount <= GetCost()` | `CurrentHP <= GetCost()` |
| 〃 | `DecreaseGunPowderCount(cost, ...)` | `DecreaseHP(cost, ...)` |
| **PlayerBaseState.cs** | `CurrentPlayerGunPowderCount` 참조 (4곳) | `CurrentHP` |
| 〃 | `DecreaseGunPowderCount(cost, ...)` | `DecreaseHP(cost, ...)` |
| **PlayerDamagedState.cs** | `CurrentPlayerGunPowderCount / InitGunpowderCount` | `CurrentHP / InitHP` |
| **PlayerHitStopState.cs** | `CurrentPlayerGunPowderCount / InitGunpowderCount` | `CurrentHP / InitHP` |

### 1-10. 기타 변경

| 파일 | 이전 | 현재 |
|------|------|------|
| **GunPowderBezierCurve.cs** | `RPC_RequestIncreaseGunPowder` | `RPC_RequestIncreaseGP` |
| **TutorialGunpowder.cs** | `IncreaseGunPowderCount(1)` | `IncreaseHP(1)` |

---

## 2. 다른 작업자가 사용할 수 있는 주요 함수 정리

### PlayerStat.cs

| 함수명 | 파라미터 | 동작 | 비고 |
|--------|----------|------|------|
| `DecreaseHP(int amount, int attacker, bool isNormalAttack, bool ignoreImmune)` | amount: 피해량, attacker: 공격자 ActorNr | HP 감소, 사망 시 캐리오버 적용 후 RPC 동기화. `bool` 반환 (사망 여부) | 궁극기 트리거(HP≤30) 포함 |
| `IncreaseHP(int amount)` | amount: 회복량 | HP 증가 + RPC 동기화 | 사망 상태(HP≤0)에서는 무시 |
| `DecreaseGP(int amount)` | amount: 감소량 | GP 감소 (음수 허용) + RPC 동기화 | 클램프 없음, 음수 진입 가능 |
| `IncreaseGP(int amount)` | amount: 증가량 | GP 증가 + RPC 동기화 | |
| `RPC_RequestIncreaseGP(int amount)` | amount: 증가량 | [PunRPC] GP 픽업 시 사용. 소유자만 실행 | `GunPowderBezierCurve`에서 호출 |
| `SetPlayerHPAndLife(int hp, int life)` | hp, life | 원격 클라이언트에서 HP/Life 값 동기화 | DamageChecker에서 호출 |
| `ApplyResurrectCarryover()` | 없음 (private) | 리스폰 시 GP 기반 HP/GP 조정 | DecreaseHP 내부에서 자동 호출 |

### EventManager.cs

| 이벤트명 | 시그니처 | 용도 | 발행 함수 |
|----------|----------|------|-----------|
| `OnDataChanged` | `Action<int, int, int, int>` | HP 변경 알림 (playerNumber, hp, life, attacker) | `PlayerDataChange(hp, life, playerNumber, attacker)` |
| `OnGPDataChanged` | `Action<int, int>` | GP 변경 알림 (playerNumber, gp) | `PlayerGPChange(playerNumber, gp)` |

### UI_InGameProfileSlot.cs

| 함수명 | 파라미터 | 동작 |
|--------|----------|------|
| `Init(Sprite bombImage, EInGameTeam team, PhotonPlayer player, int hp, int life, int gp)` | 기존 + gp 추가 | 슬롯 초기화. HP, GP 텍스트 모두 세팅 |
| `Refresh(int hp, int life, int attacker)` | hp, life, attacker | HP 텍스트 갱신 + 색상 변경 + 흔들림 연출 |
| `RefreshGP(int gp)` | gp | GP 텍스트 갱신 (음수 그대로 표시) |

### UI_InGameProfile.cs

| 함수명 | 파라미터 | 동작 |
|--------|----------|------|
| `RefreshGP(int playerNumber, int gp)` | playerNumber, gp | 해당 플레이어 슬롯의 GP UI 갱신 |

### RoomStatManager.cs

| 멤버 | 타입 | 값 | 비고 |
|------|------|----|------|
| `PlayerHP` | `const int` | `150` | HP 고정 상수 (static 접근: `RoomStatManager.PlayerHP`) |
| `PlayerGunpowder` | `int` | 로비 설정값 | GP 초기값으로 사용 (기존과 동일) |

---

## 3. 추가 참고 사항

### 3-1. Inspector 바인딩 필요 (필수)

`UI_InGameProfileSlot`의 필드명이 변경되었기 때문에, **프리팹 `ProfileSlot_Ingame`에서 기존 바인딩이 해제**되었습니다.

Inspector에서 아래 필드를 **반드시 다시 바인딩**해야 합니다:

- `HPTextUGUI` — HP 표시용 TextMeshProUGUI (기존 GunpowderTextUGUI가 바인딩되어있던 TMP)
- `GPTextUGUI` — GP 표시용 TextMeshProUGUI (**새로운 TMP 오브젝트 생성 후 바인딩** 필요)

### 3-2. 하위 호환성 Shim

기존 코드에서 아래 이름으로 접근하던 부분은 **shim(호환 래퍼)** 을 통해 동작합니다. 새 코드를 작성할 때는 새 이름을 사용해 주세요.

| 하위호환 (사용 가능하나 비권장) | 새 이름 (권장) |
|------|------|
| `CurrentPlayerGunPowderCount` | `CurrentHP` |
| `InitGunpowderCount` | `InitHP` |
| `DecreaseGunPowderCount(...)` | `DecreaseHP(...)` |
| `IncreaseGunPowderCount(...)` | `IncreaseHP(...)` |
| `SetPlayerGunPowderCountAndLife(...)` | `SetPlayerHPAndLife(...)` |
| `RPC_RequestIncreaseGunPowder(...)` | `RPC_RequestIncreaseGP(...)` |
| `RPC_ChangeGunpowder(...)` | `RPC_ChangeHP(...)` |

### 3-3. 피격 시 동작 흐름 변경

```
[피격 발생]
    ├── DecreaseHP(damage, attacker)     → HP 감소, 사망 판정, 캐리오버
    ├── DecreaseGP(gpDrop)               → GP 감소 (음수 허용) ← 신규
    └── ReleaseGunPowder(gpDrop, ...)    → GP 오브젝트 낙출 (기존과 동일)
```

### 3-4. 미공격 페널티 동작 흐름 변경

```
[미공격 타이머 만료]
    ├── DecreaseHP(AttackPenaltyAmount, ...)   → HP 감소
    ├── DecreaseGP(NO_ATTACK_RELEASE_COUNT)    → GP 감소 ← 신규
    └── RPC_ReleaseGunPowder(...)              → GP 오브젝트 낙출 (기존과 동일)
```

### 3-5. GP 픽업 시 동작 (중요 변경)

GP 오브젝트를 픽업하면 **HP가 아닌 GP만 증가**합니다.
- 이전: `RPC_RequestIncreaseGunPowder` → GP(=체력) 증가
- 현재: `RPC_RequestIncreaseGP` → GP(재화)만 증가, HP 변동 없음

### 3-6. GP 이벤트 체인

```
PlayerStat.DecreaseGP / IncreaseGP
    → RPC_ChangeGP (All Clients)
        → EventManager.PlayerGPChange(actorNumber, gp)
            → OnGPDataChanged 이벤트
                → UI_InGameProfile.RefreshGP
                    → UI_InGameProfileSlot.RefreshGP
```

### 3-7. 변경된 파일 목록 (17개)

| # | 파일 | 변경 유형 |
|---|------|-----------|
| 1 | `Player/PlayerStat.cs` | 핵심 로직 전면 개편 |
| 2 | `Player/PlayerDamageController.cs` | 피격 로직 HP/GP 분리 |
| 3 | `Player/PlayerGunpowderController.cs` | 미공격 페널티 GP 추가 |
| 4 | `Player/Player.cs` | 이벤트 구독 이름 변경 |
| 5 | `Player/PlayerUltimateController.cs` | HP 기준으로 변경 |
| 6 | `Player/PlayerHealthBar.cs` | HP 이벤트 구독 변경 |
| 7 | `Player/State/PlayerBaseState.cs` | HP 참조로 변경 |
| 8 | `Player/State/PlayerDamagedState.cs` | HP 비율 계산 변경 |
| 9 | `Player/State/PlayerHitStopState.cs` | HP 비율 계산 변경 |
| 10 | `Evenet/EventManager.cs` | GP 이벤트 추가 |
| 11 | `Photon/DamageChecker.cs` | 점수 계산 HP 기준 변경 |
| 12 | `Photon/RoomStatManager.cs` | HP 상수 추가 |
| 13 | `Map/UI/UI_InGameProfile.cs` | GP 이벤트 구독 + RefreshGP |
| 14 | `Map/UI/UI_InGameProfileSlot.cs` | HP/GP TMP 분리 |
| 15 | `UI/UI_GunPowderStatus.cs` | HP 이벤트 구독 변경 |
| 16 | `GunPowder/GunPowderBezierCurve.cs` | GP 픽업 RPC 변경 |
| 17 | `Tutorial/TutorialGunpowder.cs` | IncreaseHP로 변경 |
