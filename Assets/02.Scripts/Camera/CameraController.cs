using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private ProCamera2D _proCamera;

    private Player _target;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_proCamera == null)
        {
            _proCamera = _mainCamera.GetComponent<ProCamera2D>();
        }

        if (_proCamera == null)
        {
            Debug.LogError("메인카메라에 ProCamera2D 컴포넌트가 없습니다.");
        }
    }

    public void SetTarget(Player player)
    {
        if (_target != null)
        {
            _target.OnHit -= HitShake;
            _target.OnAttack += GunShotShake;
        }
        _target = player;
        _target.OnHit += HitShake;
        _target.OnAttack += GunShotShake;

        _proCamera.RemoveAllCameraTargets();
        _proCamera.AddCameraTarget(player.transform);
    }

    private void HitShake()
    {
        ProCamera2DShake.Instance.Shake("PlayerHit");
    }

    private void GunShotShake()
    {
        ProCamera2DShake.Instance.Shake("GunShot");
    }

    public void ExplosionShake(Transform explosionTransform, float explosionRadius)
    {
        if(_target == null) return;
        float distance = Vector3.Distance(_target.transform.position, explosionTransform.position);
        if (distance < explosionRadius * 1.8f)
        {
            ProCamera2DShake.Instance.Shake("LargeExplosion");
            return;
        }
        if (distance < explosionRadius * 6f)
        {
            ProCamera2DShake.Instance.Shake("SmallExplosion");
        }
    }

    private void OnDisable()
    {
        if (_target != null)
        {
            _target.OnHit -= HitShake;
            _target.OnAttack -= GunShotShake;
            _target = null;
        }

        if (_proCamera != null)
        {
            _proCamera.RemoveAllCameraTargets();
        }
    }
}
