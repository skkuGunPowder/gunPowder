using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class RacingCar : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider2D _hitCollider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Explosion _explosionPrefab;

    [Header("사운드")]
    [SerializeField] private string _waitSoundName;
    [SerializeField] private string _waitTireSoundName;
    [SerializeField] private string _moveSoundName;
    [SerializeField] private string _hornSoundName;

    [Header("대기 설정")]
    [SerializeField] private float _readyAnimDuration = 0.5f;
    [SerializeField] private float _waitDuration = 3f;

    [Header("이동 설정")]
    [SerializeField] private float _speed = 30f;

    [Header("페이드 설정")]
    [SerializeField] private float _fadeDuration = 0.2f;
    [SerializeField] private float _destroyDelay = 0.5f;

    private int _direction;
    private float _leftBound;
    private float _rightBound;
    private float _spawnOffset;
    
    private Sound _waitSoundInstance;
    private Sound _waitTireSoundInstance;
    private Tween _moveTween;
    private Tween _fadeTween;
    private CancellationTokenSource _cts;
    private readonly HashSet<int> _hitActorNumbers = new HashSet<int>();
    private bool _isFading;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        _direction = (int)data[0];
        _leftBound = (float)data[1];
        _rightBound = (float)data[2];
        _spawnOffset = data.Length > 3 ? (float)data[3] : 0f;

        // 방향 표시는 RacingCarSpawner가 PhotonNetwork.Instantiate의 rotation으로 전달함
        // (direction 1 → Y=180, direction -1 → Y=0)
        _hitCollider.enabled = false;

        WaitPhase().Forget();
    }

    private async UniTaskVoid WaitPhase()
    {
        CancellationToken token = _cts.Token;
        try
        {
            // AnyState → Start 진입 (준비 애니메이션 1회)
            _animator.SetTrigger("Start");

            if (!string.IsNullOrEmpty(_waitSoundName))
            {
                _waitSoundInstance = SoundManager.Instance.PlayLocalSound(_waitSoundName, transform);
            }
            
            if (!string.IsNullOrEmpty(_waitTireSoundName))
            {
                _waitTireSoundInstance = SoundManager.Instance.PlayLocalSound(_waitTireSoundName, transform);
            }

            await UniTask.WaitForSeconds(_readyAnimDuration, cancellationToken: token);

            // Start → Wait 전이 (대기 애니메이션 반복)
            _animator.SetTrigger("Wait");
            
            float remainingWait = _waitDuration - _readyAnimDuration;
            if (remainingWait > 0f)
            {
                await UniTask.WaitForSeconds(remainingWait, cancellationToken: token);
            }

            MovePhase();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void MovePhase()
    {
        _animator.SetTrigger("Go");

        if (_waitSoundInstance != null)
        {
            _waitSoundInstance.Stop();
            _waitSoundInstance = null;
        }
        
        if (_waitTireSoundInstance != null)
        {
            _waitTireSoundInstance.Stop();
            _waitTireSoundInstance = null;
        }

        // 모든 클라이언트가 자신의 로컬 플레이어와의 충돌을 감지하기 위해 collider 활성화
        _hitCollider.enabled = true;

        SoundManager.Instance.PlayLocalSound(_moveSoundName, transform);
        if (!photonView.IsMine) return;


        float targetX = _direction == 1
            ? _rightBound + _spawnOffset
            : _leftBound - _spawnOffset;
        float duration = Mathf.Abs(targetX - transform.position.x) / _speed;

        _moveTween = transform.DOMoveX(targetX, duration).SetEase(Ease.Linear)
            .OnComplete(() => photonView.RPC(nameof(RPC_StartFadeOut), RpcTarget.All));

    }

    [PunRPC]
    private void RPC_StartFadeOut() => StartFadeOut();

    private void StartFadeOut()
    {
        if (_isFading) return;
        _isFading = true;

        if (_hitCollider != null)
        {
            _hitCollider.enabled = false;
        }

        if (_spriteRenderer != null)
        {
            _fadeTween = _spriteRenderer.DOFade(0f, _fadeDuration);
        }

        DelayedDestroy().Forget();
    }

    private async UniTaskVoid DelayedDestroy()
    {
        CancellationToken token = _cts.Token;
        try
        {
            await UniTask.WaitForSeconds(_destroyDelay, cancellationToken: token);

            if (photonView != null && photonView.IsMine && photonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFading) return;
        if (!other.CompareTag("Player")) return;

        // 각 클라이언트가 "자기 자신의 플레이어가 차에 부딪힌 경우"만 감지
        PhotonView otherPv = other.GetComponentInParent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        int victimActorNr = otherPv.OwnerActorNr;
        Vector3 carPos = transform.position;
        Vector3 victimPos = other.transform.position;

        // 모든 클라이언트에서 폭발을 생성해야, 피해자 본인 클라에서 PlayerDamageController.TakeDamage의
        // IsMine 가드를 통과하고 데미지 RPC가 발화됨.
        photonView.RPC(nameof(RPC_ExplodeAt), RpcTarget.All,
            victimActorNr, carPos, victimPos);
    }

    [PunRPC]
    private void RPC_ExplodeAt(int victimActorNr, Vector3 carPos, Vector3 victimPos)
    {
        if (_isFading) return;
        if (!_hitActorNumbers.Add(victimActorNr)) return; // 같은 차 → 같은 플레이어 중복 방지 (각 클라이언트 로컬)

        Vector3 midpoint = (carPos + victimPos) * 0.5f;
        
        if (!string.IsNullOrEmpty(_hornSoundName))
        {
            SoundManager.Instance.PlayLocalSound(_hornSoundName, transform);
        }

        Explosion explosion = ExplosionPool.Instance.Get(_explosionPrefab.name);
        explosion.transform.position = midpoint;
        explosion.Explode(true, photonView);
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _moveTween?.Kill();
        _moveTween = null;

        _fadeTween?.Kill();
        _fadeTween = null;

        if (_waitSoundInstance != null)
        {
            _waitSoundInstance.Stop();
            _waitSoundInstance = null;
        }
        
        if (_waitTireSoundInstance != null)
        {
            _waitTireSoundInstance.Stop();
            _waitTireSoundInstance = null;
        }
    }
}
