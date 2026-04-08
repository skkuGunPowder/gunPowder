using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerGunpowderController : MonoBehaviour
{
    private Player _player;
    private PlayerStat _playerStat;
    private PhotonView _photonView;
    private PlayerFSM _playerFSM;
    [SerializeField]
    private PlayerSFXAnimationEvent _playerSFXAnimationEvent;

    [Header("타이머")]
    [SerializeField]
    private float _attackTimer = 0f;
    public float AttackTimer => _attackTimer;
    [SerializeField]
    private float _gunPowderDecreaseTimer = 0f;
    public float GunPowderDecreaseTimer => _gunPowderDecreaseTimer;
    [SerializeField]
    private float _gunPowderDecreaseWithoutAttackTimer;
    public float GunPowderDecreaseWithoutAttackTimer => _gunPowderDecreaseWithoutAttackTimer;
    [SerializeField]
    private float _colorUpdateWithoutAttackTimer = 0f;
    private float _warningSfxTimer = 0f;

    // 공격 없을 때 경고 상태 동기화
    private bool _isNoAttackWarningActive = false;
    private float _syncedWarningStartTime = 0f; // 경고 시작 시간 (PhotonNetwork.Time 기준)

    // 공격 없을 때 관련 상수
    private const float COLOR_UPDATE_TICK_SECONDS = 0.5f;
    private const float REDNESS_START_RATIO = 0.4f;
    private const float WARNING_RATIO_THRESHOLD = 0.7f;
    private const float MAX_RED_SATURATION = 0.6f;
    private const float WARNING_INTERVAL_MAX = 0.7f;
    private const float WARNING_INTERVAL_MIN = 0.1f;
    private const float WARNING_PITCH_MIN = 1.0f;
    private const float WARNING_PITCH_MAX = 1.9f;
    private const int NO_ATTACK_RELEASE_COUNT = 10;
    private const float NO_ATTACK_RELEASE_SPREAD_ANGLE = 30f;
    private const float NO_ATTACK_RELEASE_DISTANCE = 1.0f;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerStat = GetComponent<PlayerStat>();
        _photonView = GetComponent<PhotonView>();
        _playerFSM = GetComponent<PlayerFSM>();
    }

    private void Update()
    {
        // Owner 전용 로직 (타이머 관리)
        // AttackTimer는 웨이팅룸에서도 증가시켜야 쿨타임이 정상 작동함
        if (_photonView != null && _photonView.IsMine)
        {
            _attackTimer += Time.deltaTime;
        }

        // 대기방에서 작동 안하게 하기 위해 추가
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentGameState == EGameState.Waiting
            || GameManager.Instance.CurrentGameState == EGameState.GameOver
            || GameManager.Instance.CurrentGameState == EGameState.Tutorial)
        {
            return;
        }

        // Owner 전용 로직 (데미지 처리 등)
        if (_photonView != null && _photonView.IsMine)
        {
            // 공격 없을 때 건파우더 감소
            if (!_playerStat.IsPausedNoAttack)
            {
                _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;
            }
            DecreaseGunPowderWithoutAttack();

            // Gunpowder heal SFX window is managed in PlayerSFXAnimationEvent
            UpdateWarningSfx();
        }

        // 모든 클라이언트에서 실행 (시각적 효과)
        UpdateNoAttackWarningVisuals();
    }

    /// <summary>
    /// 주기적으로 건파우더 감소
    /// </summary>
    private void DecreaseGunPowderPeriodically()
    {
        if (_gunPowderDecreaseTimer >= _playerStat.GunPowderDecreaseTime)
        {
            _gunPowderDecreaseTimer = 0f;
            _playerStat.DecreaseGunPowderCount(1, _photonView.OwnerActorNr);
        }
    }

    /// <summary>
    /// 공격을 일정시간 하지 않으면 건파우더 감소 (Owner만 실행)
    /// </summary>
    private void DecreaseGunPowderWithoutAttack()
    {
        if (_gunPowderDecreaseWithoutAttackTimer >= _playerStat.AttackPenaltyTime)
        {
            _gunPowderDecreaseWithoutAttackTimer = 0f;
            _colorUpdateWithoutAttackTimer = 0f;
            _warningSfxTimer = 0f;

            // 경고 상태 종료
            RPC_SetNoAttackWarningState(false, 0f);

            // 낙사 상태면 건파우더 감소 안함
            if (_playerStat.IsFallingDead)
            {
                return;
            }

            _playerStat.DecreaseHP(_playerStat.AttackPenaltyAmount, _photonView.OwnerActorNr, isNormalAttack: true, ignoreImmune: true);

            // 패널티 발동 이벤트
            PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnNoAttackPenaltyTriggered();

            // GP 감소 (음수 허용)
            _playerStat.DecreaseGP(NO_ATTACK_RELEASE_COUNT);

            // 히트스크린 추가
            EventManager.Instance.HitScreen();

            _player.RPC_ReleaseGunPowder(transform.position, _photonView.OwnerActorNr, NO_ATTACK_RELEASE_COUNT, NO_ATTACK_RELEASE_SPREAD_ANGLE, NO_ATTACK_RELEASE_DISTANCE, true);
            if (_photonView.IsMine && _player.ExplosionEffectPrefab != null)
            {
                _player.RPC_PlayExplosionEffect();
            }

            // 슈퍼아머 상태가 아닐 때만 DamagedState로 전환
            if (_playerFSM != null && !_player.IsSuperArmorEnabled)
            {
                _playerFSM.SyncStateChange<PlayerDamagedState>();
            }
        }
        else
        {
            // 경고 상태 확인 및 동기화
            float ratio = _gunPowderDecreaseWithoutAttackTimer / _playerStat.AttackPenaltyTime;

            // 경고 시작 (ratio가 0.4 이상일 때)
            if (ratio >= REDNESS_START_RATIO && !_isNoAttackWarningActive)
            {
                RPC_SetNoAttackWarningState(true, (float)PhotonNetwork.Time);
                PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnNoAttackPenaltyStart();
            }
            // 경고 종료 (ratio가 0.4 미만일 때)
            else if (ratio < REDNESS_START_RATIO && _isNoAttackWarningActive)
            {
                RPC_SetNoAttackWarningState(false, 0f);
            }
        }
    }

    /// <summary>
    /// 공격 없을 때 경고 상태 동기화 RPC
    /// </summary>
    private void RPC_SetNoAttackWarningState(bool isActive, float startTime)
    {
        if (_photonView == null || !_photonView.IsMine)
        {
            return;
        }
        _photonView.RPC(nameof(SetNoAttackWarningState), RpcTarget.All, isActive, startTime);
    }

    [PunRPC]
    private void SetNoAttackWarningState(bool isActive, float startTime)
    {
        _isNoAttackWarningActive = isActive;
        _syncedWarningStartTime = startTime;

        if (!isActive)
        {
            // 경고 종료 시 펄스 효과 중단 (내부에서 색상 복원 처리됨)
            _player.StopPreExplosionPulse(true);
            _colorUpdateWithoutAttackTimer = 0f;
        }
    }

    /// <summary>
    /// 모든 클라이언트에서 실행되는 경고 시각 효과 업데이트
    /// </summary>
    private void UpdateNoAttackWarningVisuals()
    {
        if (!_isNoAttackWarningActive)
        {
            return;
        }

        // 경고 시작 이후 경과 시간 계산
        float elapsedTime = (float)PhotonNetwork.Time - _syncedWarningStartTime;
        float ratio = REDNESS_START_RATIO + (elapsedTime / _playerStat.AttackPenaltyTime);
        ratio = Mathf.Clamp01(ratio);

        // 0.5초 간격으로만 색 업데이트
        _colorUpdateWithoutAttackTimer += Time.deltaTime;
        if (_colorUpdateWithoutAttackTimer >= COLOR_UPDATE_TICK_SECONDS)
        {
            _colorUpdateWithoutAttackTimer = 0f;

            // 비율에 따라 펄스 시작/정지 (경고 단계)
            if (ratio >= WARNING_RATIO_THRESHOLD)
            {
                // 이미 재생 중이 아닐 때만 시작
                if (!_player.IsPreExplosionPulseActive())
                {
                    _player.PlayPreExplosionPulse();
                }
            }
            else
            {
                // 재생 중일 때만 정지
                if (_player.IsPreExplosionPulseActive())
                {
                    _player.StopPreExplosionPulse(false);
                }
            }

            // ratio 0.4~1 -> S: 0~0.8로 맵핑 (H=0 고정, V는 유지)
            float t = Mathf.Clamp01((ratio - REDNESS_START_RATIO) / (1f - REDNESS_START_RATIO));
            float targetS = Mathf.Lerp(0f, MAX_RED_SATURATION, t);

            // 원본 색상을 기반으로 빨간색 적용
            _player.UpdateWarningColor(targetS);
        }
    }

    // 경고음: ratio가 0.7 이상일 때 점점 빠른 간격으로 재생
    private void UpdateWarningSfx()
    {
        if (_playerSFXAnimationEvent == null)
        {
            return;
        }
        float ratio = _gunPowderDecreaseWithoutAttackTimer / _playerStat.AttackPenaltyTime;
        if (ratio < WARNING_RATIO_THRESHOLD)
        {
            _warningSfxTimer = 0f;
            return;
        }

        // WARNING_RATIO_THRESHOLD → 1.0 사이에서 재생 간격을 선형으로 WARNING_INTERVAL_MAX → WARNING_INTERVAL_MIN로 축소, 피치 WARNING_PITCH_MIN → WARNING_PITCH_MAX로 상승
        float t = Mathf.InverseLerp(WARNING_RATIO_THRESHOLD, 1f, Mathf.Clamp01(ratio));
        float interval = Mathf.Lerp(WARNING_INTERVAL_MAX, WARNING_INTERVAL_MIN, t);
        float pitch = Mathf.Lerp(WARNING_PITCH_MIN, WARNING_PITCH_MAX, t);
        _warningSfxTimer += Time.deltaTime;
        if (_warningSfxTimer >= interval)
        {
            _warningSfxTimer = 0f;
            // 로컬 소유자만 재생
            if (_photonView.IsMine)
            {
                _playerSFXAnimationEvent.PlayerWithoutAttackSFX(pitch);
            }
        }
    }

    /// <summary>
    /// 색상과 시각적 효과를 모두 초기화하는 메서드 (로컬 효과만)
    /// </summary>
    private void ResetColorAndEffects()
    {
        // 경고 상태 종료 (로컬 변수만 초기화, RPC는 별도 호출)
        _isNoAttackWarningActive = false;
        _syncedWarningStartTime = 0f;

        // 펄스 효과 중단 및 스케일 리셋
        _player.StopPreExplosionPulse(true);

        // 스프라이트 색상을 원본 색상으로 초기화
        _player.RestoreOriginalColors();

        // 타이머들 초기화
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _colorUpdateWithoutAttackTimer = 0f;
        _warningSfxTimer = 0f;
    }

    /// <summary>
    /// 공격 없음 경고를 완전히 해제 (네트워크 동기화 + 로컬 효과)
    /// 공격 시, 궁극기 활성화 시, 부활 시 등에 사용
    /// </summary>
    public void ClearNoAttackWarning()
    {
        if (_photonView != null && _photonView.IsMine)
        {
            RPC_SetNoAttackWarningState(false, 0f);
            PlayerEventManager.Instance.GetEvents(_photonView.OwnerActorNr).InvokeOnNoAttackPenaltyReset();
        }
        ResetColorAndEffects();
    }

    /// <summary>
    /// 공격을 하면 타이머 초기화 (외부 호출용 public 메서드)
    /// </summary>
    public void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        ClearNoAttackWarning();
    }

    public void SetPausedNoAttack()
    {
        _playerStat.IsPausedNoAttack = true;
        ClearNoAttackWarning();
    }

    public void ResetPausedNoAttack()
    {
        _playerStat.IsPausedNoAttack = false;
    }

    /// <summary>
    /// 타이머 초기화 (부활 시 호출)
    /// </summary>
    public void ResetTimers()
    {
        _attackTimer = 0f;
        _gunPowderDecreaseTimer = 0f;
        _gunPowderDecreaseWithoutAttackTimer = 0f;
        _colorUpdateWithoutAttackTimer = 0f;
        _warningSfxTimer = 0f;
    }
}
