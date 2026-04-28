using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 건파우더 양에 따라 UI 색상, 이미지, 파티클 위치를 동적으로 변경하는 스크립트
/// </summary>
public class UI_GunPowderStatus : MonoBehaviour
{
    // 건파우더 상태 enum
    private enum GunPowderState
    {
        High,   // > 50
        Middle, // 31-50
        Low     // <= 30
    }

    [Header("UI References")]
    [SerializeField] private Image _targetUIImage; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetOutLineUIImage; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetUIImage2; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetOutLineUIImage2; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _fuseImage; // Fuse 오브젝트의 이미지
    
    [Header("Fuse Sprites")]
    [SerializeField] private Sprite _coolTimeHigh;   // 건파우더 > 50
    [SerializeField] private Sprite _coolTimeMiddle; // 건파우더 <= 50
    [SerializeField] private Sprite _coolTimeLow;    // 건파우더 <= 30
    
    [Header("Particle Transforms")]
    [SerializeField] private Transform _fuseHighParticle;   // 건파우더 > 50일 때 파티클 위치
    [SerializeField] private Transform _fuseMiddleParticle; // 건파우더 <= 50일 때 파티클 위치
    [SerializeField] private Transform _fuseLowParticle;    // 건파우더 <= 30일 때 파티클 위치
    [SerializeField] private Transform _particleSystem;      // 이동할 파티클 시스템 (RectTransform)
    
    [Header("Color Settings")]
    [SerializeField] private Color _colorHigh = new Color(1f, 0.627f, 0.627f, 1f);   // #FFA0A0
    [SerializeField] private Color _colorMiddle = new Color(1f, 0.38f, 0.38f, 1f);   // #FF6161
    [SerializeField] private Color _colorLow = new Color(1f, 0f, 0f, 1f);            // #FF0000
    
    [Header("Threshold Settings")]
    [SerializeField] private int _middleThreshold = 50; // 이 값 이하면 Middle 상태
    [SerializeField] private int _lowThreshold = 30;    // 이 값 이하면 Low 상태
    
    private Player _player;
    private GunPowderState _currentState = GunPowderState.High; // 현재 상태 추적 (중복 업데이트 방지)
    
    private void Start()
    {
        // EventManager의 PlayerListUp 이벤트 구독
        EventManager.Instance.OnFindPlayer += FindAndSubscribeToPlayer;
    }
    
    /// <summary>
    /// 로컬 플레이어 찾기 및 건파우더 이벤트 구독
    /// </summary>
    private void FindAndSubscribeToPlayer(GameObject player)
    {
        if (player != null)
        {
            _player = player.GetComponent<Player>();
            
            if (_player != null)
            {
                PlayerStat playerStat = _player.GetComponent<PlayerStat>();
                if (playerStat != null)
                {
                    // 건파우더 변경 이벤트 구독
                    playerStat.OnHPChanged += OnGunPowderChanged;
                    
                    // 초기 상태 설정
                    OnGunPowderChanged(playerStat.CurrentHP);
                }
                else
                {
                    Debug.LogError("[UI_GunPowderStatus] GameObject에 PlayerStat 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogError("[UI_GunPowderStatus] GameObject에 Player 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[UI_GunPowderStatus] 'Player' 태그를 가진 GameObject를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 건파우더 변경 이벤트 핸들러
    /// </summary>
    /// <param name="currentGunPowder">현재 건파우더 양</param>
    private void OnGunPowderChanged(int currentGunPowder)
    {
        // 현재 건파우더 양에 따라 상태 결정
        GunPowderState newState = DetermineState(currentGunPowder);
        
        // 상태가 변경되었을 때만 UI 업데이트 (최적화)
        if (newState != _currentState)
        {
            _currentState = newState;
            UpdateUI(newState);
        }
    }
    
    /// <summary>
    /// 건파우더 양에 따라 상태 결정
    /// </summary>
    private GunPowderState DetermineState(int gunPowder)
    {
        if (gunPowder > _middleThreshold)
        {
            return GunPowderState.High;
        }
        else if (gunPowder > _lowThreshold)
        {
            return GunPowderState.Middle;
        }
        else
        {
            return GunPowderState.Low;
        }
    }
    
    /// <summary>
    /// 상태에 따라 UI 업데이트 (색상, 이미지, 파티클 위치)
    /// </summary>
    private void UpdateUI(GunPowderState state)
    {
        switch (state)
        {
            case GunPowderState.High:
                ApplyHighState();
                break;
            case GunPowderState.Middle:
                ApplyMiddleState();
                break;
            case GunPowderState.Low:
                ApplyLowState();
                break;
        }
    }
    
    /// <summary>
    /// High 상태 적용 (건파우더 > 50)
    /// </summary>
    private void ApplyHighState()
    {
        // UI 색상 변경
        if (_targetUIImage != null)
        {
            _targetUIImage.color = _colorHigh;
        }
        if (_targetOutLineUIImage != null)
        {
            _targetOutLineUIImage.color = _colorHigh;
        }
        if (_targetUIImage2 != null)
        {
            _targetUIImage2.color = _colorHigh;
        }
        if (_targetOutLineUIImage2 != null)
        {
            _targetOutLineUIImage2.color = _colorHigh;
        }
        
        // Fuse 이미지 변경
        if (_fuseImage != null && _coolTimeHigh != null)
        {
            _fuseImage.sprite = _coolTimeHigh;
            _fuseImage.color = _colorHigh; // Fuse 색상도 변경
        }
        
        // 파티클 위치 변경
        if (_particleSystem != null && _fuseHighParticle != null)
        {
            _particleSystem.position = _fuseHighParticle.position;
        }
        
        Debug.Log("[UI_GunPowderStatus] High 상태 적용: 색상 FFA0A0, 이미지 CoolTime_High");
    }
    
    /// <summary>
    /// Middle 상태 적용 (건파우더 31-50)
    /// </summary>
    private void ApplyMiddleState()
    {
        // UI 색상 변경
        if (_targetUIImage != null)
        {
            _targetUIImage.color = _colorMiddle;
        }
        if (_targetOutLineUIImage != null)
        {
            _targetOutLineUIImage.color = _colorMiddle;
        }
        if (_targetUIImage2 != null)
        {
            _targetUIImage2.color = _colorMiddle;
        }
        if (_targetOutLineUIImage2 != null)
        {
            _targetOutLineUIImage2.color = _colorMiddle;
        }
        
        // Fuse 이미지 변경
        if (_fuseImage != null && _coolTimeMiddle != null)
        {
            _fuseImage.sprite = _coolTimeMiddle;
            _fuseImage.color = _colorMiddle; // Fuse 색상도 변경
        }
        
        // 파티클 위치 변경
        if (_particleSystem != null && _fuseMiddleParticle != null)
        {
            _particleSystem.position = _fuseMiddleParticle.position;
        }
        
        Debug.Log("[UI_GunPowderStatus] Middle 상태 적용: 색상 FF6161, 이미지 CoolTime_Middle");
    }
    
    /// <summary>
    /// Low 상태 적용 (건파우더 <= 30)
    /// </summary>
    private void ApplyLowState()
    {
        // UI 색상 변경
        if (_targetUIImage != null)
        {
            _targetUIImage.color = _colorLow;
        }
        if (_targetOutLineUIImage != null)
        {
            _targetOutLineUIImage.color = _colorLow;
        }
        if (_targetUIImage2 != null)
        {
            _targetUIImage2.color = _colorLow;
        }
        if (_targetOutLineUIImage2 != null)
        {
            _targetOutLineUIImage2.color = _colorLow;
        }
        
        // Fuse 이미지 변경
        if (_fuseImage != null && _coolTimeLow != null)
        {
            _fuseImage.sprite = _coolTimeLow;
            _fuseImage.color = _colorLow; // Fuse 색상도 변경
        }
        
        // 파티클 위치 변경
        if (_particleSystem != null && _fuseLowParticle != null)
        {
            _particleSystem.position = _fuseLowParticle.position;
        }
        
        Debug.Log("[UI_GunPowderStatus] Low 상태 적용: 색상 FF0000, 이미지 CoolTime_Low");
    }
    
    private void OnDestroy()
    {
        // EventManager 이벤트 구독 해제
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnFindPlayer -= FindAndSubscribeToPlayer;
        }
        
        // Player 이벤트 구독 해제
        if (_player != null)
        {
            PlayerStat playerStat = _player.GetComponent<PlayerStat>();
            if (playerStat != null)
            {
                playerStat.OnHPChanged -= OnGunPowderChanged;
            }
        }
    }
}

