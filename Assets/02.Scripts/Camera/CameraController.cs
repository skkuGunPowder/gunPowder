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
    
    [Header("마지막 킬 관련")] 
    [Tooltip("시간 고치면 플레이어 라스트 다이 시간도 고쳐야함")]
    public float TargetZoomDuration = 1.5f;
    public float TargetZoomAmount = 1.5f;
    
    public event Action<bool> OnUIOnOff;                 // UI On/Off
    public event Action<string> OnNicknameChanged; // 타겟 이름 전달

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
        EventManager.Instance.OnTargetChanged += SetObserveTarget;
        EventManager.Instance.OnLastAttack += LastAttack;
        EventManager.Instance.OnPlayObserve += PlayObservingMode;
    }

    private void Init()
    {
        _currentTargetList = new List<Player>();
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

    private void SetObserveTarget() // 시작할 때, 리스트 최신화
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _currentTargetList.Clear();

        foreach (var player in players)
        {
            _currentTargetList.Add(player);
        }
    }
    
    private void PlayObservingMode() // 로컬에서 알아서 각자 처리, 죽었을 때 나갔을 때 리스트 최신화
    {
        if (_isObserving)
        {
            return;
        }
        
        _isObserving = true;
        OnUIOnOff?.Invoke(true);
        
        // foreach (var player in _currentTargetList)
        // {
        //     if (player.gameObject.activeSelf == false)
        //     {
        //         // activefalse가 자기 자신이면 오저버모드
        //         if (player.GetComponent<PhotonView>().Owner.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        //         {
        //             if (_isObserving)
        //             {
        //                 continue;
        //             }
        //         }
        //     }
        // }

        _currentTargetIndex = 0;
    }

    private void LastAttack(int actorNumber)
    {
        OnUIOnOff?.Invoke(false);
        
        foreach (Player p in _currentTargetList)
        {
            if (p == null)
            {
                continue;
            }
            
            PhotonPlayer photonPlayer = p.GetComponent<PhotonView>().Owner;
            if (photonPlayer.ActorNumber == actorNumber)
            {
                _proCamera.RemoveAllCameraTargets();
                _proCamera.AddCameraTarget(p.transform);
                _proCamera.Zoom(-TargetZoomAmount, TargetZoomDuration);
                EventManager.Instance.GameSet();
                _currentTargetList.Clear();
                return;
            }
        }
    }
    
    private void Update()
    {
        if (_isObserving == false)
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

    public void SelectTarget(int index)
    {
        if (_currentTargetList.Count <= 1)
        {
            return;
        }
    
        int attempts = 0;
        int maxAttempts = _currentTargetList.Count; // 무한 루프 방지
    
        while (attempts < maxAttempts)
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
        
            // 유효한 타겟을 찾았으면 루프 종료
            if (_currentTargetList[_currentTargetIndex] != null && 
                _currentTargetList[_currentTargetIndex].gameObject.activeSelf)
            {
                break;
            }
        
            attempts++;
        }
    
        // 모든 타겟이 유효하지 않으면 리스트 정리
        if (attempts >= maxAttempts)
        {
            _currentTargetList.RemoveAll(target => target == null || !target.gameObject.activeSelf);
        
            if (_currentTargetList.Count == 0)
            {
                return;
            }
        
            _currentTargetIndex = 0;
        }

        
        Player player = _currentTargetList[_currentTargetIndex];
        
        string nickname = player.GetComponent<PhotonView>().Owner.NickName;
        OnNicknameChanged?.Invoke(nickname);
        
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
        EventManager.Instance.OnTargetChanged -= SetObserveTarget;
        EventManager.Instance.OnPlayObserve -= PlayObservingMode;
    }
}
