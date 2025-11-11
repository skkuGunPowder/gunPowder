using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ultimate : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pressCButtonUI;
    [SerializeField] private Image _pressCButtonImage;
    
    [Header("Image Sprites")]
    [SerializeField] private Sprite _pressCRed;
    [SerializeField] private Sprite _pressCYellow;
    
    [Header("Settings")]
    [SerializeField] private float _blinkInterval = 0.3f; // 깜빡임 간격
    
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
            
            isRed = !isRed;
            yield return new WaitForSeconds(_blinkInterval);
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
