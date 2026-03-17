# 건파우더(GP) 기획 변경 작업 계획서

> 작성일: 2025-03-17 (v4 수정: 2025-03-17)
> 분기: KKH_Test

---

## 1. 기획 변경 요약

**핵심 변경: HP(체력) 신규 추가 + GP는 재화 전용으로 전환**

```
[현재]  GP = 체력 + 드롭/픽업(체력 회복)
                ↓
[변경후] HP = 체력 (기존 GP 체력 로직을 그대로 이전)
         GP = 재화 (때리면 흡수, 맞으면 낙출, 상점에서 사용)
```

### 확정된 요구사항

| 항목 | 내용 |
|------|------|
| HP 사망 로직 | 기존 GP와 동일 (HP 0 → 라이프 -1 → HP 리셋 → 라이프 소진 시 사망) |
| HP 초기값 | **150 고정** (설정 불가) |
| GP 초기값 | 로비 방 설정에서 지정 (기존 GP양 설정 그대로 유지) |
| 피격 시 | **HP 감소 + GP 낙출 동시** 발생 |
| 미공격 페널티 | **HP 직접 감소 + GP 낙출** |
| 궁극기 트리거 | **HP 기준** (HP ≤ 30) |
| 사망 시 GP | **유지** (리셋 안 됨) |
| GP 픽업 | +1 재화 (체력 회복 아님) |
| GP 사용처 | 인게임 상점 재화 |
| GP 범위 | **최대/최소 없음, 음수 가능** |

### 라운드(리스폰) 시 GP 캐리오버 규칙

사망 후 다음 라이프 시작 시, 직전 GP 잔액에 따라 처리:

| 조건 | 처리 |
|------|------|
| GP ≥ 0 | GP = **현재GP + 50** 으로 시작 |
| GP < 0 (GP ≥ -100) | 리스폰 시 HP에 **\|GP\|만큼 데미지** 적용, GP = **50**으로 리셋 |
| GP < -100 | 리스폰 시 HP에 **최대 100 데미지** 적용, GP = **50**으로 리셋 |

**예시:**

```
[GP = 30일 때 사망]  → 리스폰: HP=150, GP=80 (30+50)
[GP = 0일 때 사망]   → 리스폰: HP=150, GP=50 (0+50)
[GP = -40일 때 사망]  → 리스폰: HP=110 (150-40), GP=50
[GP = -150일 때 사망] → 리스폰: HP=50 (150-100), GP=50  ← 최대 100 데미지 캡
```

---

## 2. 작업 단계

### Phase 1: PlayerStat.cs — 핵심 데이터 분리

기존 GP 체력 필드/메서드를 HP로 리네이밍하고, GP 재화 필드를 신규 추가.

**리네이밍 (로직 변경 없음):**
- `_currentPlayerGunPowderCount` → `_currentHP`
- `_initGunpowderCount` → `_initHP` (값: **150 고정**, 방 설정 무시)
- `DecreaseGunPowderCount()` → `DecreaseHP()`
- `IncreaseGunPowderCount()` → `IncreaseHP()`
- `OnGunPowderEmpty/Changed/Increased` → `OnHPEmpty/Changed/Increased`
- `RPC_ChangeGunpowder` → `RPC_ChangeHP`

**HP 초기화 변경:**
- `_initHP = 150` 상수로 고정 (RoomStatManager에서 읽지 않음)
- `ERoomProperties.Gunpowder` 방 설정은 GP 초기값으로 사용

**GP 재화 신규 추가:**
- `_currentGP` (첫 라운드 초기값: **로비 방 설정의 GP양**)
- `DecreaseGP()`, `IncreaseGP()` — **음수 허용, 클램프 없음**
- `RPC_ChangeGP`, `RPC_RequestIncreaseGP`
- `OnGPChanged` 이벤트

**리스폰 시 GP 캐리오버 로직 (신규):**
- GP ≥ 0 → GP = 현재GP + 50
- GP < 0 → HP에 min(|GP|, 100) 데미지 적용, GP = 50

> 첫 라운드만 로비 설정값, 이후 리스폰부터 캐리오버 규칙 적용

### Phase 2: PlayerDamageController.cs — 피격 이중 처리

피격 시 HP 감소와 GP 낙출을 동시에 처리.

- `DecreaseGunPowderCount` → `DecreaseHP` 호출
- GP 낙출량 = `FloorToInt(maxDamage * StealPercent / 100)` (CeilToInt → **FloorToInt**)
- `DecreaseGP(낙출량)` 호출 **(신규)** — GP가 음수로 내려갈 수 있음
- `ReleaseGunPowder(낙출량)` — GP 오브젝트 생성 (기존, 낙출량 그대로)

> GP 잔액 초과 방지 없음 — GP는 음수 허용이므로 낙출량만큼 그대로 차감하고, 오브젝트도 그대로 생성

### Phase 3: PlayerGunpowderController.cs — 미공격 페널티 이중 처리

- `DecreaseHP(AttackPenaltyAmount)` — HP 직접 감소
- `DecreaseGP(NO_ATTACK_RELEASE_COUNT)` — GP 10 차감 **(신규)** — 음수 허용
- `RPC_ReleaseGunPowder(NO_ATTACK_RELEASE_COUNT)` — GP 오브젝트 10개 생성 (기존)

### Phase 4: GunPowderBezierCurve.cs — 픽업 대상 변경

- `RPC_RequestIncreaseGunPowder` → `RPC_RequestIncreaseGP` (1줄 변경)
- GP 픽업이 체력 회복 → 재화 증가로 전환

### Phase 5: 네트워크 동기화

- `DamageChecker.cs`: 파라미터명 gunpowder → hp, 스코어 계산 HP 기반
- `EventManager.cs`: `OnDataChanged` 시맨틱 변경 (hp, life, attacker)
- `RoomStatManager.cs`:
  - `PlayerGunpowder` → **GP 초기값으로 유지** (로비 설정 그대로)
  - `PlayerHP` 신규 추가 (150 고정값)
- GP는 `RPC_ChangeGP`로 별도 동기화

### Phase 6: UI

- `PlayerHealthBar.cs`: `OnGunPowderChanged` → `OnHPChanged` 구독
- `UI_GunPowderStatus.cs`: HP 기반 색상 상태 유지
- `UI_InGameProfile.cs`: HP 표시로 변경
- **GP UI**: 기존 TMP text에 `CurrentGP` 값 바인딩 (음수 시 `-100` 등 그대로 표시)
- 인게임 상점은 추후 "증강" 시스템으로 추가 예정 — 이번 작업에서 제외

### Phase 7: 상태/궁극기/Player.cs

- 모든 상태 클래스: `CurrentPlayerGunPowderCount` → `CurrentHP`
- 궁극기: HP 기준 트리거/코스트
- `Player.cs`: 이벤트 구독 변경
- `TutorialGunpowder.cs`: `IncreaseGunPowderCount` → `IncreaseHP`

### Phase 8: 리스폰 GP 캐리오버 (신규)

사망 후 리스폰 처리 시 GP 캐리오버 로직 추가.

**처리 위치:** `PlayerStat.ResurrectPlayerStat()` 또는 `Player.HandleGunPowderEmpty()`에서 리스폰 시점에 실행

**로직:**
1. 리스폰 시 HP = 150으로 리셋 (기존 동작)
2. GP 캐리오버 적용:
   - GP ≥ 0 → GP = 현재GP + 50
   - GP < 0 → 리스폰 직후 HP에 min(|GP|, 100) 데미지 적용, GP = 50
3. HP/GP 변경 후 네트워크 동기화 (RPC_ChangeHP, RPC_ChangeGP)

**주의:** 리스폰 시 HP 데미지가 150을 넘을 수 없으므로 (최대 100 데미지) 리스폰 즉사는 불가능

---

## 3. 파일 변경 요약

| 파일 | 규모 | 핵심 변경 |
|------|------|----------|
| `PlayerStat.cs` | 🔴 대규모 | GP 재화 추가, GP→HP 리네이밍, HP=150 고정, 리스폰 캐리오버 |
| `PlayerDamageController.cs` | 🟡 중간 | HP감소+GP낙출 이중 처리, FloorToInt |
| `PlayerGunpowderController.cs` | 🟡 중간 | HP감소+GP낙출 이중 처리 |
| `Player.cs` | 🟡 중간 | 이벤트/위임 변경, 리스폰 캐리오버 적용 |
| `GunPowderBezierCurve.cs` | 🟢 소규모 | RPC 1줄 변경 |
| `DamageChecker.cs` | 🟢 소규모 | 파라미터명 변경 |
| `EventManager.cs` | 🟢 소규모 | 시맨틱 변경 |
| `RoomStatManager.cs` | 🟢 소규모 | PlayerHP(150 고정) 추가, PlayerGunpowder는 GP 초기값으로 유지 |
| `PlayerHealthBar.cs` | 🟢 소규모 | 이벤트 구독 변경 |
| `UI_GunPowderStatus.cs` | 🟢 소규모 | 이벤트 구독 변경 |
| `UI_InGameProfile.cs` | 🟢 소규모 | 참조 변경 |
| `PlayerBaseState.cs` 등 상태 클래스 | 🟢 소규모 | 참조 변경 |
| `PlayerUltimateController.cs` | 🟢 소규모 | HP 기준으로 변경 |
| `TutorialGunpowder.cs` | 🟢 소규모 | 메서드명 변경 |

---

## 4. 엣지 케이스

| 상황 | 처리 |
|------|------|
| GP=0일 때 피격 | GP 음수로 진입, GP 오브젝트는 정상 생성 |
| GP=음수일 때 피격 | GP 더 내려감, GP 오브젝트 정상 생성 |
| GP=0일 때 미공격 페널티 | GP=-10, GP 오브젝트 10개 생성 |
| 사망 시 GP ≥ 0 | 리스폰: HP=150, GP=현재GP+50 |
| 사망 시 GP < 0 (≥ -100) | 리스폰: HP=150-\|GP\|, GP=50 |
| 사망 시 GP < -100 | 리스폰: HP=50 (150-100), GP=50 — 데미지 캡 100 |
| 최종 사망 (라이프 0) | HP=0, GP 유지 (결과 화면용) |
| GP 범위 | **최대/최소 제한 없음, 음수 허용** |

---

## 5. 확정된 추가 사항

| # | 항목 | 결정 |
|---|------|------|
| A | 인게임 상점 | 추후 **"증강"** 시스템으로 별도 추가 예정 — 이번 작업 범위 밖 |
| B | GP UI 표시 | 기존 UI 사용 (TMP text 숫자만 변경하면 됨) |
| C | GP 음수 표현 | 그대로 음수 표시 (예: `-100`) |
| D | 첫 라운드 GP | **로비 설정값** 그대로 시작 |
