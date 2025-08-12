using System;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private ProCamera2D _proCamera;

    private Player _target;
    
    private List<Player> _currentTargetList = new List<Player>();
    private int _currentTargetIndex = 0;
    private void Awake()
    {
        Init();
        
    }

    private void Start()
    {
        if (GameManager.Instance.CurrentGameState == EGameState.Waiting)
        {
            return;
        }

        EventManager.Instance.OnTargetChanged += TargetListUp;
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

    public void SmallShakeAt(Transform shakeTransform, float radius)
    {
        if (_target == null) return;
        float distance = Vector3.Distance(_target.transform.position, shakeTransform.position);
         if (distance < radius * 6f)
        {
            ProCamera2DShake.Instance.Shake("SmallExplosion");
        }
    }

    private void TargetListUp()
    {
        PlayerFSM[] playerStates = FindObjectsByType<PlayerFSM>(FindObjectsSortMode.None);
        _currentTargetList.Clear();
        foreach (var fsm in playerStates)
        {
            if (fsm.IsCurrentState<PlayerObserveState>())
            {
                continue;
            }
            
            Player player = fsm.GetComponent<Player>();
            _currentTargetList.Add(player);
        }
        
        Debug.Log($"타겟으로 정할 수 있는 플레이어 수 {_currentTargetList.Count}");
    }

    public void SelectTarget(int index)
    {
        _currentTargetIndex += index;
        if (_currentTargetIndex < 0)
        {
            _currentTargetIndex = _currentTargetList.Count - 1;
        }
        else if (_currentTargetIndex >= _currentTargetList.Count)
        {
            _currentTargetIndex = 0;
        }
        
        Player player = _currentTargetList[_currentTargetIndex];
        SetTarget(player);
    }
    // private void TargetChange(Player player)
    // {
    //     _target = player;
    // }
    
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
        
        EventManager.Instance.OnTargetChanged -= TargetListUp;
    }
}
