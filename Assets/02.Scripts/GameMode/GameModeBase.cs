using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GameModeBase : MonoBehaviour
{
    /// <summary>
    /// 게임 모드 모두가 사용할 공통 함수
    /// 1. 게임 종료 : 각 규칙에 따른 종료
    /// 2. 부활 지점 설정, Get
    /// 3. 처음 플레이어 소환
    /// </summary>
    [Header("스폰, 부활")]
    [SerializeField] private Transform _resurrectPoint; 
    
    protected PlayerSpawner _playerSpawner;
    protected PhotonView _photonView;
    
    /// <summary>
    /// 스폰 포인트 설정하기
    /// </summary>
    protected virtual void Awake()
    {
        if (_playerSpawner == null)
        {
            _playerSpawner = GetComponent<PlayerSpawner>();
        }

        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }
    }
    
    protected virtual void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        
    }

    /// <summary>
    /// 부활 지점이 변경되어야 하는 경우 다른 Transform으로 교체
    /// </summary>
    public void SetResurrectPoint(Transform point)
    {
        _resurrectPoint = point;
    }
    
    /// <summary>
    /// 부활 지점 Get
    /// </summary>
    public Transform GetResurrectPoint()
    {
        return _resurrectPoint;
    }
    /// <summary>
    /// 게임 종료 시키기 : 바로 게임 종료 연출이 나옴
    /// 각 게임 규칙에 따라 호출해주면 됨
    /// </summary>
    public virtual void GameOver()
    { 
        // 연출 종료
        GameManager.Instance.RequestGameOver();
    }    
}
