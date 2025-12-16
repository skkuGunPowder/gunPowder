using UnityEngine;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using System.Collections;

public class PlayerUltimateController : MonoBehaviour
{
    private Player _player;
    private PlayerStat _playerStat;
    private PlayerSFXAnimationEvent _playerSFXAnimationEvent;
    private PhotonView _photonView;

    private Ultimate _ultimate;
    public Ultimate Ultimate => _ultimate;

    [SerializeField]
    private GameObject UltimateEffectPrefab;

    private bool _ultimateEffectOn = false;
    private Coroutine _ultimateEffectOffRoutine;

    [SerializeField]
    private float _ultimateChanceTimer = 0f;
    public float UltimateChanceTimer { get => _ultimateChanceTimer; set => _ultimateChanceTimer = value; }

    // Player의 이벤트에 접근하기 위한 참조
    public event System.Action OnUltimateChanceActivated;
    public event System.Action OnUltimateChanceDeactivated;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerStat = GetComponent<PlayerStat>();
        _playerSFXAnimationEvent = GetComponent<PlayerSFXAnimationEvent>();
        _photonView = GetComponent<PhotonView>();
    }

    private void OnDestroy()
    {
        if (UltimateManager.Instance != null && _ultimate != null)
        {
            UltimateManager.Instance.ReturnUltimate(_ultimate);
            _ultimate = null;
        }
    }

    /// <summary>
    /// 궁극기 설정 (LoadItems에서 호출)
    /// </summary>
    public void SetUltimate(Ultimate ultimate)
    {
        _ultimate = ultimate;
    }

    /// <summary>
    /// 궁극기 사용 가능 상태 타이머 업데이트 (Update에서 호출)
    /// </summary>
    public void UpdateUltimateChanceTimer()
    {
        // 궁극기 사용가능 상태
        if (_playerStat.HasUltimateChance)
        {
            if (!_ultimateEffectOn)
            {
                if (UltimateEffectPrefab != null && !UltimateEffectPrefab.activeSelf)
                {
                    SetUltimateEffectState(true);
                }
            }

            _ultimateChanceTimer += Time.deltaTime;
            if (_ultimateChanceTimer >= _playerStat.UltimateChanceDuration)
            {
                _playerStat.HasUltimateChance = false;
                _playerStat.HasUsedUltimateThisLife = true;
                _ultimateChanceTimer = 0f;

                SetUltimateEffectState(false);
            }
        }
    }

    /// <summary>
    /// 궁극기 효과 활성화/비활성화 (Material + VFX + 이벤트)
    /// </summary>
    public void SetUltimateEffectState(bool isActive, bool invokeEvent = true)
    {
        if (isActive)
        {
            // 궁극기 활성화 시 경고 상태 해제
            _player.ClearNoAttackWarning();

            _player.RPC_SetMaterial((byte)EPlayerMaterial.Ultimate);
            RPC_UltimateEffect(true);
            _ultimateEffectOn = true;

            if (invokeEvent)
            {
                OnUltimateChanceActivated?.Invoke();
            }
        }
        else
        {
            // 궁극기 비활성화: 깜박임 완전 중단 → 색상 복원 → Material 복구 순서
            _player.ClearNoAttackWarning();  // 1. 깜박임 중단 + 타이머 초기화 + 색상 복원

            _player.RPC_SetMaterial((byte)EPlayerMaterial.Default);  // 2. Material 기본으로
            RPC_UltimateEffect(false);  // 3. 궁극기 VFX 끄기 (내부에서 색상 복원 재확인)
            _ultimateEffectOn = false;

            if (invokeEvent)
            {
                OnUltimateChanceDeactivated?.Invoke();
            }
        }
    }

    public void RPC_UltimateEffect(bool isOn)
    {
        if (_photonView == null || !_photonView.IsMine)
        {
            return;
        }
        _photonView.RPC(nameof(UltimateEffect), RpcTarget.All, isOn);
    }

    [PunRPC]
    public void UltimateEffect(bool isOn)
    {
        if (UltimateEffectPrefab == null)
        {
            return;
        }

        if (isOn)
        {
            // 궁극기 활성화 시 색상 복원 (모든 클라이언트에서 실행)
            _player.RestoreOriginalColors();

            UltimateEffectPrefab.SetActive(true);
            if (_ultimateEffectOffRoutine != null)
            {
                StopCoroutine(_ultimateEffectOffRoutine);
                _ultimateEffectOffRoutine = null;
            }
            UltimateEffectPrefab.SetActive(true);
            PlayParticleGroup(UltimateEffectPrefab);
        }
        else
        {
            // 궁극기 비활성화 시 색상 복원 (모든 클라이언트에서 실행)
            _player.RestoreOriginalColors();

            if (_ultimateEffectOffRoutine != null)
            {
                StopCoroutine(_ultimateEffectOffRoutine);
            }
            _ultimateEffectOffRoutine = StartCoroutine(StopParticleGroupThenDisable(UltimateEffectPrefab));
        }
    }

    private void PlayParticleGroup(GameObject root)
    {
        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) { continue; }
            var emission = ps.emission;
            emission.enabled = true;
            ps.Play(true);
        }
    }

    private IEnumerator StopParticleGroupThenDisable(GameObject root)
    {
        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) { continue; }
            var emission = ps.emission;
            emission.enabled = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        bool anyAlive = true;
        while (anyAlive)
        {
            anyAlive = false;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (ps != null && ps.IsAlive(true))
                {
                    anyAlive = true;
                    break;
                }
            }
            yield return null;
        }

        root.SetActive(false);
        _ultimateEffectOffRoutine = null;
    }

    public void ExecuteUltimate()
    {
        if (_playerStat.HasUltimateChance && !_playerStat.HasUsedUltimateThisLife)
        {
            if (_ultimate == null)
            {
                Debug.LogError("궁극기 스크립트를 찾을 수 없습니다.");
                return;
            }

            // 피가 부족하면 궁극기 사용 불가
            if (_playerStat.CurrentPlayerGunPowderCount <= _ultimate.GetCost())
            {
                return;
            }

            _ultimate.ExcuteUltimate();
            _playerStat.HasUsedUltimateThisLife = true;
            _playerStat.HasUltimateChance = false;
            _ultimateChanceTimer = 0f;
            int ultimateCost = _ultimate.GetCost();
            _playerStat.DecreaseGunPowderCount(ultimateCost, _photonView.OwnerActorNr);

            // 궁극기 효과 비활성화 (내부에서 경고도 자동으로 해제됨)
            SetUltimateEffectState(false);

            // SFX
            _playerSFXAnimationEvent.PlayerUltimateUseSFX();

            // 궁극기 연출
            _photonView.RPC(nameof(Rpc_UltimateProduction), RpcTarget.All, _ultimate.GetBombID());
        }
    }

    [PunRPC]
    public void Rpc_UltimateProduction(string bomb, PhotonMessageInfo info)
    {
        PhotonPlayer player = info.Sender;
        EventManager.Instance.Ultimate(bomb, player);
    }

    /// <summary>
    /// 강제로 궁극기 사용가능상태 만들기
    /// 이때는 궁극기 사용가능 시간이 무제한이다.
    /// </summary>
    public void ForceUltimateChance()
    {
        _playerStat.HasUltimateChance = true;
        _playerStat.HasUsedUltimateThisLife = false;
        _ultimateChanceTimer = 0f;
        _playerStat.UltimateChanceDuration = 999999999f;

        SetUltimateEffectState(true);
    }

    /// <summary>
    /// 궁극기 효과가 켜져있는지 확인
    /// </summary>
    public bool IsUltimateEffectOn()
    {
        return _ultimateEffectOn;
    }

    /// <summary>
    /// 궁극기 타이머 초기화 (부활 시 호출)
    /// </summary>
    public void ResetUltimateChanceTimer()
    {
        _ultimateChanceTimer = 0f;
    }
}
