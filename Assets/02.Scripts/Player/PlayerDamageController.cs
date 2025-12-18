using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어 피격/데미지 처리 전담 컨트롤러
/// - 데미지 계산
/// - 팀 판정
/// - 건파우더 드랍 (ReleaseGunPowder)
/// - 피격 VFX / SFX
/// - 데미지 팝업
/// - 공격자용 히트 파티클
/// </summary>
public class PlayerDamageController : MonoBehaviour
{
    private Player _player;
    private PlayerStat _playerStat;
    private PhotonView _photonView;
    private DamagePopup _damagePopup;

    [Header("건파우더 설정")]
    [SerializeField]
    private float _gunPowderSpreadAngle = 90f;
    private float _gunPowderSpreadDistance = 1.0f;

    [Header("컴포넌트 참조")]
    [SerializeField]
    private PlayerSFXAnimationEvent _playerSFXAnimationEvent;

    // 폭발당 히트 파티클 수집용
    private class PendingExplosionHit
    {
        public List<Vector3> victimPositions = new List<Vector3>();
        public List<int> victimViewIds = new List<int>(); // 피격자 ViewID (FollowVFX용)
        public List<bool> victimIsCrits = new List<bool>(); // 각 피격자의 크리티컬 여부 (개별 파티클용)
        public bool hasCrit = false; // 폭발에 크리티컬이 하나라도 있는지 (공격자 이펙트/사운드용)
    }

    private PendingExplosionHit _currentExplosionHit;
    private Coroutine _processHitCoroutine;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerStat = GetComponent<PlayerStat>();
        _photonView = GetComponent<PhotonView>();
        _damagePopup = GetComponent<DamagePopup>();

        // 인스펙터에 지정되지 않았다면 자동으로 가져오기
        if (_playerSFXAnimationEvent == null)
        {
            _playerSFXAnimationEvent = GetComponent<PlayerSFXAnimationEvent>();
        }
    }

    /// <summary>
    /// 외부에서 호출하는 데미지 진입 메서드
    /// </summary>
    public void TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb,
        int attackerViewId, int attackerActorNumber, bool isFallingOut, bool isNormalAttack)
    {
        if (_photonView == null || !_photonView.IsMine)
        {
            return;
        }

        EventManager.Instance.HitScreen();
        // 모든 클라이언트에서 VFX와 데미지 처리를 동기화
        _photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All,
            damage, maxDamage, HealPercent, attackerBomb, attackerViewId,
            attackerActorNumber, isFallingOut, isNormalAttack);
    }

    /// <summary>
    /// 피격 VFX와 사운드를 재생하는 RPC 메서드
    /// </summary>
    [PunRPC]
    public void RPC_PlayHitEffects(int damage, int maxDamage)
    {
        // 피격 VFX 재생
        if (VFXPool.Instance != null)
        {
            Vector3 vfxPos = transform.position + _player.PlayerHitVFXOffset;
            if (tag == "Player")
            {
                VFXPool.Instance.RandomPlay("Damaged", vfxPos, 1, 3);
            }
            else
            {
                VFXPool.Instance.RandomPlay("Hit", vfxPos, 1, 6);
            }
        }

        // 맥스 데미지를 받았을때 다른 사운드 재생
        if (damage == maxDamage)
        {
            if (_playerSFXAnimationEvent != null)
            {
                _playerSFXAnimationEvent.PlayerCritDamageVoiceRandomSFX();
            }
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f,
                false, SoundType.SFX, true, 1f, 50f);
        }
        else
        {
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f,
                false, SoundType.SFX, true, 1f, 50f);
            SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 3, 0f,
                false, SoundType.SFX, true, 1f, 50f);
        }
    }

    /// <summary>
    /// 실제 데미지 계산/적용 및 네트워크 동기화
    /// </summary>
    [PunRPC]
    public void RPC_TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb,
        int attackerViewId, int attackerActorNumber, bool isFallingOut, bool isNormalAttack,
        PhotonMessageInfo info)
    {
        if (_playerStat == null || _playerStat.IsImmune)
        {
            return;
        }

        // 공격자 정보 가져오기
        PhotonView attackerView = PhotonView.Find(attackerViewId);

        // 같은 팀 체크
        bool isSameTeam = false;
        if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy)
        {
            PlayerStat attackerStat = attackerView.GetComponent<PlayerStat>();

            // 팀 비교는 동기화된 로컬 필드 사용 (시작 시 CustomProperties로부터 동기화됨)
            EInGameTeam attackerTeam = attackerStat != null ? attackerStat.Team : EInGameTeam.Default;
            EInGameTeam victimTeam = _playerStat.Team;

            // 팀 체크: 같은 팀이면서 자기 자신이 아닌 경우
            isSameTeam = (attackerTeam == victimTeam && attackerActorNumber != _photonView.OwnerActorNr);
        }
        else
        {
            Debug.LogWarning($"[RPC_TakeDamage] 공격자 뷰를 찾을 수 없습니다. ID: {attackerViewId}");
        }

        // 같은 팀이 아닐 때만 데미지 적용
        if (!isSameTeam)
        {
            // 피격 횟수 증가
            _playerStat.IncreseDamagedCount();

            // 건파우더 드랍량 계산 (힐량 계산)
            float healPercent = HealPercent / 100f;
            int gunPowderCount = Mathf.CeilToInt(maxDamage * healPercent);

            // 플레이어가 맞은 횟수에 비례해서 데미지 증가
            int increaseDamagePerDamagedCount = _playerStat.CurrentPlayerDamagedCount / 15;
            damage += increaseDamagePerDamagedCount;
            maxDamage += increaseDamagePerDamagedCount;

            // 체력 감소
            bool isDead = _playerStat.DecreaseGunPowderCount(damage, attackerActorNumber, isNormalAttack);

            // 날 때린 사람 딜량 증가 (자기 자신일 경우 제외)
            if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy)
            {
                PlayerStat attackerStat = attackerView.GetComponent<PlayerStat>();
                if (attackerStat != null && attackerView != _photonView)
                {
                    attackerStat.IncreaseTotalDamage(damage);
                    /*
                    if (isDead)
                    {
                        // 킬 카운트는 공격자 본인의 클라이언트에서만 증가시키도록 RPC 호출
                        if (attackerView.Owner != null)
                        {
                            attackerView.RPC(nameof(PlayerStat.RPC_IncreaseTotalKillCount), attackerView.Owner);
                        }
                    }*/
                }
            }

            // Gunpowder 낙출
            ReleaseGunPowder(attackerBomb, attackerViewId, gunPowderCount,
                _gunPowderSpreadAngle, _gunPowderSpreadDistance, isFallingOut);
        }   
        else
        {
            // 같은 팀일 때 데미지 0으로 설정
            if (GameManager.Instance.CurrentGameState == EGameState.Playing)
            {
                damage = 0;
            }
        }

        // 거리 기반 데미지 비율 저장 및 피격 이벤트 발생
        _player.RegisterHitDamage(damage, maxDamage);

        // 데미지 팝업 & VFX/사운드: 중복 호출 방지
        // 오직 RPC_TakeDamage를 원래 보낸 클라이언트(피격자 Owner)에서만 RPC를 전송한다
        if (info.Sender != null && info.Sender.IsLocal)
        {
            // 맞은 사람(Owner)에게 VFX/사운드와 데미지 팝업 표시
            if (_photonView.Owner != null)
            {
                _photonView.RPC(nameof(RPC_PlayHitEffects), _photonView.Owner, damage, maxDamage);
                // 같은 팀이 아닐 때만 데미지 팝업 표시
                if (!isSameTeam)
                {
                    _photonView.RPC(nameof(ShowDamagePopup), _photonView.Owner, -damage, maxDamage);
                }
            }
            // 때린 사람(Attacker Owner)에게 VFX/사운드와 데미지 팝업 표시 (피해자와 동일 Owner면 중복 방지)
            if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy &&
                attackerView.Owner != null && attackerView.Owner != _photonView.Owner)
            {
                _photonView.RPC(nameof(RPC_PlayHitEffects), attackerView.Owner, damage, maxDamage);
                // 같은 팀이 아닐 때만 데미지 팝업 표시
                if (!isSameTeam)
                {
                    _photonView.RPC(nameof(ShowDamagePopup), attackerView.Owner, damage, maxDamage);
                }

                // 공격자에게 히트 파티클 생성 요청 (로컬에서만 실행)
                if (!isSameTeam)
                {
                    bool isCrit = (damage == maxDamage);
                    attackerView.RPC(nameof(SpawnAttackerHitParticles), attackerView.Owner,
                        transform.position, isCrit, _photonView.ViewID);
                }
            }
        }
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    [PunRPC]
    public void ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3,
        float spreadAngle = 30f, float distance = 1.0f, bool isFallingOut = true)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // attackerViewId로 Transform 찾기
        Transform attacker = null;
        PhotonView attackerView = PhotonView.Find(attackerViewId);
        if (attackerView != null)
            attacker = attackerView.transform;

        Vector3 baseDir = (transform.position - explosionOrigin).normalized;

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * spreadAngle;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * baseDir;
            Vector3 spawnPos = transform.position + dir * distance;
            spawnPos.z = 0f;

            // 랜덤 시드 추가 (시간 + 인덱슬 고유값 생성)
            int randomSeed = UnityEngine.Random.Range(0, 9999);

            object[] instData = new object[] { attackerViewId, isFallingOut, randomSeed, _photonView.ViewID };
            PhotonNetwork.Instantiate(_player.GunPowderPrefab.name, spawnPos, Quaternion.identity, 0, instData);
        }
    }

    /// <summary>
    /// 외부에서 호출하는 Gunpowder Release RPC 래퍼
    /// </summary>
    public void RPC_ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3,
        float spreadAngle = 30f, float distance = 1.0f, bool isFallingOut = true)
    {
        if (_photonView == null || !_photonView.IsMine)
        {
            return;
        }

        _photonView.RPC(nameof(ReleaseGunPowder), RpcTarget.All,
            explosionOrigin, attackerViewId, count, spreadAngle, distance, isFallingOut);
    }

    [PunRPC]
    public void ShowDamagePopup(int value, int maxDamage)
    {
        if (_damagePopup == null)
        {
            _damagePopup = GetComponent<DamagePopup>();
        }
        if (_damagePopup == null)
        {
            return;
        }
        _damagePopup.SpawnPopup(value, maxDamage);
    }

    /// <summary>
    /// 공격자(자신)가 적을 맞췄을 때 파티클 생성 (로컬에서만 실행)
    /// 폭발당 1개의 이펙트만 생성하기 위해 0.05초 동안 수집 후 일괄 처리
    /// </summary>
    /// <param name="victimPosition">피격자 위치</param>
    /// <param name="isCrit">크리티컬 여부</param>
    /// <param name="victimViewId">피격자 PhotonView ID (FollowVFX용)</param>
    [PunRPC]
    public void SpawnAttackerHitParticles(Vector3 victimPosition, bool isCrit, int victimViewId)
    {
        if (VFXPool.Instance == null)
        {
            Debug.LogWarning("[PlayerDamageController] VFXPool.Instance is null. Cannot spawn hit particles.");
            return;
        }

        // 1. 데이터 수집 시작 (첫 번째 피격)
        if (_currentExplosionHit == null)
        {
            _currentExplosionHit = new PendingExplosionHit();

            // 0.05초 후에 일괄 처리하기로 예약
            if (_processHitCoroutine != null)
            {
                StopCoroutine(_processHitCoroutine);
            }
            _processHitCoroutine = StartCoroutine(ProcessHitsDelayed());
        }

        // 2. 피격 정보 수집
        _currentExplosionHit.victimPositions.Add(victimPosition);
        _currentExplosionHit.victimViewIds.Add(victimViewId);
        _currentExplosionHit.victimIsCrits.Add(isCrit); // 각 피격자의 크리티컬 여부 저장
        if (isCrit)
        {
            _currentExplosionHit.hasCrit = true; // 한 명이라도 크리티컬이면 true (공격자 이펙트/사운드용)
        }
    }

    /// <summary>
    /// 0.05초 대기 후 수집된 히트 정보를 일괄 처리
    /// </summary>
    private IEnumerator ProcessHitsDelayed()
    {
        // 같은 프레임의 모든 피격 정보를 수집하기 위해 대기
        yield return new WaitForSeconds(0.05f);

        if (_currentExplosionHit == null)
        {
            _processHitCoroutine = null;
            yield break;
        }

        bool explosionHasCrit = _currentExplosionHit.hasCrit; // 폭발에 크리티컬이 있는지 (공격자 이펙트/사운드용)

        // 1. 각 피격자를 따라다니는 파티클 생성 (플레이어마다 개별 파티클!)
        for (int i = 0; i < _currentExplosionHit.victimViewIds.Count; i++)
        {
            int victimViewId = _currentExplosionHit.victimViewIds[i];
            bool individualCrit = _currentExplosionHit.victimIsCrits[i]; // 각 피격자의 크리티컬 여부

            // 각 피격자에 맞는 파티클 선택
            GameObject particlePrefab = individualCrit
                ? _player.PlayerCritHitParticlePrefab
                : _player.PlayerHitParticlePrefab;

            if (particlePrefab != null)
            {
                PhotonView victimView = PhotonView.Find(victimViewId);

                if (victimView != null && victimView.gameObject.activeInHierarchy)
                {
                    // FollowVFX로 피격자를 따라다니게 함
                    FollowVFX vfx = VFXPool.Instance.Get(particlePrefab.name) as FollowVFX;
                    if (vfx != null)
                    {
                        vfx.PlayAttached(victimView.transform);
                    }
                }
                else
                {
                    // 피격자를 찾을 수 없으면 위치에 고정
                    Vector3 pos = _currentExplosionHit.victimPositions[i];
                    VFXPool.Instance.Play(particlePrefab.name, pos);
                }
            }
        }

        // 2. 자신 우측에 이펙트 생성 (폭발당 1개만! 크리티컬 우선)
        GameObject hitPrefab = explosionHasCrit
            ? _player.PlayerCritHitPrefab
            : _player.PlayerHitPrefab;
        if (hitPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.right * _player.PlayerHitParticleOffset;
            VFXPool.Instance.Play(hitPrefab.name, spawnPos);
        }

        /*
        // 3. 사운드 재생 (폭발당 1번만! 크리티컬 우선)
        if (SoundManager.Instance != null)
        {
            string soundName = explosionHasCrit ? "PlayerCrit_1" : "PlayerHit_1";
            SoundManager.Instance.PlayLocalSound(soundName, transform, 0f, false, SoundType.SFX, true, 1f, 50f);
        }*/

        // 정리
        _currentExplosionHit = null;
        _processHitCoroutine = null;
    }
}

