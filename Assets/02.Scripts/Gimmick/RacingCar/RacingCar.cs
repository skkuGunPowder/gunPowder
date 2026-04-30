using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class RacingCar : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider2D _hitCollider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Explosion _explosionPrefab;

    [Header("사운드")]
    [SerializeField] private string _readySoundName;
    [SerializeField] private string _waitSoundName;
    [SerializeField] private string _moveSoundName;

    [Header("대기 설정")]
    [SerializeField] private float _readyAnimDuration = 0.5f;
    [SerializeField] private float _waitDuration = 2f;

    [Header("이동 설정")]
    [SerializeField] private float _speed = 30f;

    private int _direction;
    private float _leftBound;
    private float _rightBound;
    private Sound _waitSoundInstance;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        _direction = (int)data[0];
        _leftBound = (float)data[1];
        _rightBound = (float)data[2];

        // direction 1 = 좌측 생성 → 우측으로, -1 = 우측 생성 → 좌측으로
        _spriteRenderer.flipX = _direction == -1;

        _hitCollider.enabled = false;

        WaitPhase().Forget();
    }

    private async UniTaskVoid WaitPhase()
    {
        SoundManager.Instance.PlayLocalSound(_readySoundName, transform);

        await UniTask.WaitForSeconds(_readyAnimDuration);

        _animator.SetTrigger("Wait");
        _waitSoundInstance = SoundManager.Instance.PlayLocalSound(_waitSoundName, transform, isLoop: true);

        float remainingWait = _waitDuration - _readyAnimDuration;
        if (remainingWait > 0f)
        {
            await UniTask.WaitForSeconds(remainingWait);
        }

        MovePhase();
    }

    private void MovePhase()
    {
        _animator.SetTrigger("Go");

        if (_waitSoundInstance != null)
        {
            _waitSoundInstance.Stop();
        }

        // SoundManager.Instance.PlayLocalSound(_moveSoundName, transform);

        _hitCollider.enabled = true;

        float targetX = _direction == 1 ? _rightBound : _leftBound;
        float duration = Mathf.Abs(targetX - transform.position.x) / _speed;

        transform.DOMoveX(targetX, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine) { return; }
        if (!other.CompareTag("Player")) { return; }

        Vector3 midpoint = (transform.position + other.transform.position) * 0.5f;

        Explosion explosion = ExplosionPool.Instance.Get(_explosionPrefab.name);
        explosion.transform.position = midpoint;
        explosion.Explode(false, photonView);
        
    }
}
