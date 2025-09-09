using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private ProCamera2D _proCamera;

    private Player _target;

    private bool _isObserving = false;
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
        EventManager.Instance.OnPlayerListUp += TargetListUp;
        EventManager.Instance.OnTargetChanged += TargetListUp;
        EventManager.Instance.OnLastAttack += LastAttack;
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
        if (GameManager.Instance.LastPlayer)
        {
            return;
        }
        
        Debug.Log("TargetListUp");
        
        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _currentTargetList.Clear();
        
        foreach (var player in players)
        {
            if (player.gameObject.activeSelf == false)
            {
                // activefalse가 자기 자신이면 오저버모드
                if (player.GetComponent<PhotonView>().Owner.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    _isObserving = true;
                };
                continue;
            }
            
            _currentTargetList.Add(player);
            
        }
        
        Debug.Log($"target list : {_currentTargetList.Count}");
        EventManager.Instance.OnPlayerListUp -= TargetListUp;
    }

    private void LastAttack(PhotonPlayer player)
    {
        Debug.Log("LastAttack");
        
        foreach (Player p in _currentTargetList)
        {
            PhotonPlayer photonPlayer = p.GetComponent<PhotonView>().Owner;
            if (photonPlayer.ActorNumber == player.ActorNumber)
            {
                _proCamera.RemoveAllCameraTargets();
                _proCamera.AddCameraTarget(p.transform, duration: 1.5f);
                Debug.Log($"player : {photonPlayer.ActorNumber}");
                
                StartCoroutine(TestCoroutine());
                return;
            }
        }
    }
    
    private IEnumerator TestCoroutine()
    {
        Debug.Log("TestCoroutine");
        yield return new WaitForSeconds(5);
        GameManager.Instance.RequestGameOver();
    }
    private void Update()
    {
        if (_isObserving == false || GameManager.Instance.LastPlayer)
        {
            return;
        }
        
        if (InputHandler.GetKeyDown(KeyCode.LeftArrow) || InputHandler.GetKeyDown(KeyCode.DownArrow))
        {
            SelectTarget(-1);
        }

        if (InputHandler.GetKeyDown(KeyCode.RightArrow)|| InputHandler.GetKeyDown(KeyCode.UpArrow))
        {
            SelectTarget(1);
        }

    }

    private void SelectTarget(int index)
    {
        if (_currentTargetList.Count <= 1)
        {
            return;
        }
        
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
        EventManager.Instance.OnLastAttack -= LastAttack;
        EventManager.Instance.OnTargetChanged -= TargetListUp;
    }
}
