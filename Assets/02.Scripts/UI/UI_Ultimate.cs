using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ultimate : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pressCButtonUI;
    [SerializeField] private Image _pressCButtonImage;
    
    [SerializeField] private Image _targetUIImage; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetOutLineUIImage; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetUIImage2; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _targetOutLineUIImage2; // 색상을 변경할 UI 이미지
    [SerializeField] private Image _fuseImage; // Fuse 오브젝트의 이미지
    
    [Header("Image Sprites")]
    [SerializeField] private Sprite _pressCRed;
    [SerializeField] private Sprite _pressCYellow;
    
    [Header("Settings")]
    [SerializeField] private float _blinkInterval = 0.3f; // 깜빡임 간격
    
    [Header("Color Settings")]
    [SerializeField] private Color _colorYellow = new Color(0.984f, 0.816f, 0.212f, 1f); // #FBD036
    [SerializeField] private Color _colorRed = new Color(1f, 0f, 0f, 1f);                // #FF0000
    [SerializeField] private Color _colorHigh = new Color(1f, 0.627f, 0.627f, 1f);       // #FFA0A0
    [SerializeField] private Color _colorMiddle = new Color(1f, 0.38f, 0.38f, 1f);       // #FF6161
    [SerializeField] private Color _colorLow = new Color(1f, 0f, 0f, 1f);                // #FF0000
    
    [Header("Threshold Settings")]
    [SerializeField] private int _middleThreshold = 50; // 이 값 이하면 Middle 상태
    [SerializeField] private int _lowThreshold = 30;    // 이 값 이하면 Low 상태
    
    private Player _player;
    private Coroutine _blinkCoroutine;
    
    private void Start()
    {
        // 초기 상태 설정
        if (_pressCButtonUI != null)
        {
            _pressCButtonUI.SetActive(false);
        }
        
        if (_pressCButtonImage != null && _pressCRed != null)
        {
            _pressCButtonImage.sprite = _pressCRed;
        }
        
        // EventManager의 PlayerListUp 이벤트 구독
        EventManager.Instance.OnPlayerListUp += OnPlayerListUp;
    }
    
    /// <summary>
    /// 플레이어 리스트 업데이트 이벤트 핸들러 (플레이어 생성 후 호출됨)
    /// </summary>
    private void OnPlayerListUp()
    {
        FindAndSubscribeToPlayer();
    }
    
    /// <summary>
    /// 로컬 플레이어 찾기 및 궁극기 이벤트 구독
    /// </summary>
    private void FindAndSubscribeToPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.GetComponent<Player>();
            
            if (_player != null)
            {
                // 궁극기 이벤트 구독
                _player.OnUltimateChanceActivated += OnUltimateActivated;
                _player.OnUltimateChanceDeactivated += OnUltimateDeactivated;
            }
            else
            {
                Debug.LogError("[UI_Ultimate] GameObject에 Player 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[UI_Ultimate] 'Player' 태그를 가진 GameObject를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 궁극기 활성화 이벤트 핸들러
    /// </summary>
    private void OnUltimateActivated()
    {
        ActivateUltimateUI();
    }
    
    /// <summary>
    /// 궁극기 비활성화 이벤트 핸들러
    /// </summary>
    private void OnUltimateDeactivated()
    {
        DeactivateUltimateUI();
    }
    
    /// <summary>
    /// 궁극기 UI 활성화
    /// </summary>
    private void ActivateUltimateUI()
    {
        Debug.Log("[UI_Ultimate] ActivateUltimateUI 호출됨");        
        if (_pressCButtonUI == null)
        {
            Debug.LogError("[UI_Ultimate] _pressCButtonUI가 null이어서 활성화 불가!");
            return;
        }
        
        // UI 활성화
        _pressCButtonUI.SetActive(true);
        
        // 깜빡임 시작
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }
    
    /// <summary>
    /// 궁극기 UI 비활성화
    /// </summary>
    private void DeactivateUltimateUI()
    {

        if (_pressCButtonUI == null)
        {
            Debug.LogError("[UI_Ultimate] _pressCButtonUI가 null이어서 비활성화 불가!");
            return;
        }
        
        // 깜빡임 중지
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
        
        // 빨간색으로 변경
        if (_pressCButtonImage != null && _pressCRed != null)
        {
            _pressCButtonImage.sprite = _pressCRed;
        }
        
        // 현재 건파우더 양에 따라 색상 복원
        RestoreColorsByGunPowder();
        
        // UI 비활성화
        _pressCButtonUI.SetActive(false);
    }
    
    /// <summary>
    /// 이미지 깜빡임 코루틴
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        bool isRed = true;
        
        while (true)
        {
            // Press C 버튼 이미지 스프라이트 변경 (빨 노 빨 노)
            if (_pressCButtonImage != null)
            {
                if (isRed && _pressCRed != null)
                {
                    _pressCButtonImage.sprite = _pressCRed;
                }
                else if (!isRed && _pressCYellow != null)
                {
                    _pressCButtonImage.sprite = _pressCYellow;
                }
            }
            else
            {
                Debug.LogError("[UI_Ultimate] BlinkCoroutine - _pressCButtonImage가 null!");
            }
            
            // 5개 이미지 색상 변경 (노 빨 노 빨 - pressCButton과 반대)
            Color currentColor = isRed ? _colorYellow : _colorRed;
            
            if (_targetUIImage != null)
            {
                _targetUIImage.color = currentColor;
            }
            if (_targetOutLineUIImage != null)
            {
                _targetOutLineUIImage.color = currentColor;
            }
            if (_targetUIImage2 != null)
            {
                _targetUIImage2.color = currentColor;
            }
            if (_targetOutLineUIImage2 != null)
            {
                _targetOutLineUIImage2.color = currentColor;
            }
            if (_fuseImage != null)
            {
                _fuseImage.color = currentColor;
            }
            
            isRed = !isRed;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }
    
    /// <summary>
    /// 현재 건파우더 양에 따라 색상 복원
    /// </summary>
    private void RestoreColorsByGunPowder()
    {
        if (_player == null)
        {
            Debug.LogError("[UI_Ultimate] _player가 null이어서 색상 복원 불가!");
            return;
        }
        
        // PlayerStat에서 현재 건파우더 양 가져오기
        PlayerStat playerStat = _player.GetComponent<PlayerStat>();
        if (playerStat == null)
        {
            Debug.LogError("[UI_Ultimate] PlayerStat 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        int currentGunPowder = playerStat.CurrentPlayerGunPowderCount;
        Color targetColor;
        
        // 건파우더 양에 따라 색상 결정
        if (currentGunPowder > _middleThreshold)
        {
            targetColor = _colorHigh; // #FFA0A0
            Debug.Log($"[UI_Ultimate] 색상 복원: High 상태 (건파우더 {currentGunPowder})");
        }
        else if (currentGunPowder > _lowThreshold)
        {
            targetColor = _colorMiddle; // #FF6161
            Debug.Log($"[UI_Ultimate] 색상 복원: Middle 상태 (건파우더 {currentGunPowder})");
        }
        else
        {
            targetColor = _colorLow; // #FF0000
            Debug.Log($"[UI_Ultimate] 색상 복원: Low 상태 (건파우더 {currentGunPowder})");
        }
        
        // 5개 이미지 색상 복원
        if (_targetUIImage != null)
        {
            _targetUIImage.color = targetColor;
        }
        if (_targetOutLineUIImage != null)
        {
            _targetOutLineUIImage.color = targetColor;
        }
        if (_targetUIImage2 != null)
        {
            _targetUIImage2.color = targetColor;
        }
        if (_targetOutLineUIImage2 != null)
        {
            _targetOutLineUIImage2.color = targetColor;
        }
        if (_fuseImage != null)
        {
            _fuseImage.color = targetColor;
        }
    }
    
    private void OnDestroy()
    {
        // EventManager 이벤트 구독 해제
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerListUp -= OnPlayerListUp;
        }
        
        // Player 이벤트 구독 해제
        if (_player != null)
        {
            _player.OnUltimateChanceActivated -= OnUltimateActivated;
            _player.OnUltimateChanceDeactivated -= OnUltimateDeactivated;
        }
        
        // 코루틴 정리
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
    }
}
