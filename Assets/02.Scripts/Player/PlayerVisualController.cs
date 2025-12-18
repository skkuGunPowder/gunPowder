using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Player의 시각 효과 / 색상 / 정렬 관련 로직을 담당하는 컨트롤러.
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    private Player _player;
    private PlayerStat _playerStat;

    [Header("죽음 파츠")]
    [SerializeField]
    private List<GameObject> _diePartList;
    public List<GameObject> DiePartList => _diePartList;
    private List<GameObject> _headPartList;
    public List<GameObject> HeadPartList => _headPartList;
    private List<GameObject> _bodyPartList;
    public List<GameObject> BodyPartList => _bodyPartList;
    private List<GameObject> _leftArmPartList;
    public List<GameObject> LeftArmPartList => _leftArmPartList;
    private List<GameObject> _leftLegPartList;
    public List<GameObject> LeftLegPartList => _leftLegPartList;
    private List<GameObject> _rightArmPartList;
    public List<GameObject> RightArmPartList => _rightArmPartList;
    private List<GameObject> _rightLegPartList;
    public List<GameObject> RightLegPartList => _rightLegPartList;

    // 경고 펄스 / 색상 제어용
    private Tween _preExplosionPulseTween;
    private Vector3 _defaultLocalScale;
    private Dictionary<SpriteRenderer, Color> _originalColorMap;          // 게임 시작 시 저장되는 진짜 원본 색상
    private Dictionary<SpriteRenderer, int> _originalSortingOrderMap;    // 스프라이트 렌더러의 원본 sortingOrder 저장
    private bool _isColorRestored = true;                                // 색상이 원본 상태인지 추적
    private readonly List<SpriteRenderer> _dieSpriteRendererList = new List<SpriteRenderer>();
    public IReadOnlyList<SpriteRenderer> DieSpriteRendererList => _dieSpriteRendererList;

    // 경고용 색상/펄스 상수 (Player와 동일 값 유지)
    private const float MAX_RED_SATURATION = 0.6f;
    private const float PULSE_SCALE_MULTIPLIER = 1.2f;
    private const float PULSE_HALF_DURATION = 0.2f;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerStat = GetComponent<PlayerStat>();
        _defaultLocalScale = transform.localScale;
    }

    #region 원본 색상 / 정렬 초기화 및 등록

    /// <summary>
    /// 플레이어의 신체 부위별 GameObject를 초기화하고 분류하는 메서드
    /// PlayerStat의 SpriteRenderer 리스트를 순회하며 BodyPartMarker 컴포넌트를 기반으로
    /// 나중에 추가될 부분도 BodyPartMarker 컴포넌트를 추가해줘야 함
    /// </summary>
    public void InitializeBodyParts()
    {
        // 각 신체 부위별 GameObject 리스트를 초기화
        _headPartList = new List<GameObject>();        // 머리 부위 리스트
        _bodyPartList = new List<GameObject>();        // 몸통 부위 리스트
        _leftArmPartList = new List<GameObject>();     // 왼팔 부위 리스트
        _leftLegPartList = new List<GameObject>();     // 왼다리 부위 리스트
        _rightArmPartList = new List<GameObject>();    // 오른팔 부위 리스트
        _rightLegPartList = new List<GameObject>();    // 오른다리 부위 리스트

        if (_diePartList == null)
        {
            return;
        }

        foreach (var part in _diePartList)
        {
            if (part == null) { continue; }  // null 체크

            // 해당 SpriteRenderer가 속한 GameObject에서 BodyPartMarker 컴포넌트 검색
            BodyPartMarker markerComp = part.GetComponent<BodyPartMarker>();
            if (markerComp == null) { continue; }  // BodyPartMarker가 없으면 스킵

            // 마커에서 정의된 신체 부위 타입 가져오기
            BodyPartType partType = markerComp.PartType;

            // 신체 부위 타입에 따라 해당하는 리스트에 GameObject 추가
            switch (partType)
            {
                case BodyPartType.Head:     // 머리 부위
                    _headPartList.Add(part);
                    break;
                case BodyPartType.Body:     // 몸통 부위
                    _bodyPartList.Add(part);
                    break;
                case BodyPartType.LeftArm:  // 왼팔 부위
                    _leftArmPartList.Add(part);
                    break;
                case BodyPartType.LeftLeg:  // 왼다리 부위
                    _leftLegPartList.Add(part);
                    break;
                case BodyPartType.RightArm: // 오른팔 부위
                    _rightArmPartList.Add(part);
                    break;
                case BodyPartType.RightLeg: // 오른다리 부위
                    _rightLegPartList.Add(part);
                    break;
            }
        }

        // 모든 신체 부위 리스트가 비어있는 경우 경고 메시지 출력
        if ((_headPartList.Count + _bodyPartList.Count + _leftArmPartList.Count + _leftLegPartList.Count +
             _rightArmPartList.Count + _rightLegPartList.Count) == 0)
        {
            Debug.LogWarning("BodyPartMarker를 찾지 못했습니다. PlayerSprites 루트에 마커를 추가해주세요.");
        }

        InitializeDieSpriteRenderers();
    }

    private void InitializeDieSpriteRenderers()
    {
        _dieSpriteRendererList.Clear();
        if (_diePartList == null)
        {
            return;
        }

        foreach (var part in _diePartList)
        {
            if (part == null) continue;

            SpriteRenderer[] spriteRenderers = part.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                RegisterDieSpriteRenderer(spriteRenderer);
            }
        }
    }

    public void RegisterDieSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || _dieSpriteRendererList.Contains(spriteRenderer))
        {
            return;
        }

        _dieSpriteRendererList.Add(spriteRenderer);
        RegisterOriginalColor(spriteRenderer);
        RegisterOriginalSortingOrder(spriteRenderer);
    }

    public void UnregisterDieSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (_dieSpriteRendererList.Contains(spriteRenderer))
        {
            _dieSpriteRendererList.Remove(spriteRenderer);
        }

        UnregisterOriginalColor(spriteRenderer);
        UnregisterOriginalSortingOrder(spriteRenderer);
    }

    /// <summary>
    /// 게임 시작 시 원본 색상을 저장합니다. (한 번만 실행)
    /// 이 색상 정보는 절대 변경되지 않으며, 모든 색상 효과가 끝날 때 이 색상으로 복원됩니다.
    /// </summary>
    public void InitializeOriginalColors()
    {
        if (_playerStat == null) { return; }

        if (_originalColorMap == null)
        {
            _originalColorMap = new Dictionary<SpriteRenderer, Color>();
        }

        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer != null && !_originalColorMap.ContainsKey(renderer))
            {
                // 원본 색상 저장 (이 값은 절대 변경되지 않음)
                _originalColorMap[renderer] = renderer.color;
            }
        }
    }

    /// <summary>
    /// 스킨 동적 추가 시 색상 시스템에 편입
    /// 주의: 스프라이트가 원본 색상 상태일 때 호출해야 합니다.
    /// </summary>
    public void RegisterOriginalColor(SpriteRenderer renderer)
    {
        if (renderer == null) { return; }
        if (_originalColorMap == null)
        {
            _originalColorMap = new Dictionary<SpriteRenderer, Color>();
        }
        if (!_originalColorMap.ContainsKey(renderer))
        {
            // 현재 색상이 빨간색(경고 상태)인지 확인
            Color currentColor = renderer.color;
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);

            // H가 0이고 S가 높으면 빨간색으로 판단 -> 흰색으로 저장
            bool isRedWarning = (h < 0.05f || h > 0.95f) && s > 0.3f;

            if (isRedWarning)
            {
                // 빨간색 경고 상태이면 기본 색상(흰색)을 원본으로 저장
                _originalColorMap[renderer] = Color.white;
                // 실제 스프라이트도 흰색으로 즉시 변경
                renderer.color = Color.white;
            }
            else
            {
                // 정상 색상이면 현재 색상을 원본으로 저장
                _originalColorMap[renderer] = currentColor;
            }
        }
    }

    public void UnregisterOriginalColor(SpriteRenderer renderer)
    {
        if (renderer == null || _originalColorMap == null) { return; }
        if (_originalColorMap.ContainsKey(renderer))
        {
            _originalColorMap.Remove(renderer);
        }
    }

    /// <summary>
    /// 스킨 동적 추가 시 sortingOrder 시스템에 편입/해제
    /// </summary>
    public void RegisterOriginalSortingOrder(SpriteRenderer renderer)
    {
        if (renderer == null) { return; }
        if (_originalSortingOrderMap == null)
        {
            _originalSortingOrderMap = new Dictionary<SpriteRenderer, int>();
        }
        if (!_originalSortingOrderMap.ContainsKey(renderer))
        {
            _originalSortingOrderMap[renderer] = renderer.sortingOrder;
        }
    }

    public void UnregisterOriginalSortingOrder(SpriteRenderer renderer)
    {
        if (renderer == null || _originalSortingOrderMap == null) { return; }
        if (_originalSortingOrderMap.ContainsKey(renderer))
        {
            _originalSortingOrderMap.Remove(renderer);
        }
    }

    /// <summary>
    /// 플레이어 시작 시 기본 스프라이트 렌더러들의 원본 sortingOrder 저장
    /// </summary>
    public void InitializeOriginalSortingOrders()
    {
        if (_playerStat?.MySpriteREndererList == null) { return; }

        foreach (var renderer in _playerStat.MySpriteREndererList)
        {
            if (renderer != null)
            {
                RegisterOriginalSortingOrder(renderer);
            }
        }
    }

    /// <summary>
    /// Player.LoadItems 안에서 호출되던 플레이어 정렬 우선순위 적용
    /// </summary>
    public void SetPlayerOrderInLayer()
    {
        if (_player == null || _playerStat == null) { return; }
        if (_originalSortingOrderMap == null)
        {
            _originalSortingOrderMap = new Dictionary<SpriteRenderer, int>();
        }

        int playerOrderInLayerPlus = _player.PhotonView.OwnerActorNr;
        foreach (var item in _playerStat.MySpriteREndererList)
        {
            if (item != null)
            {
                // 원본 sortingOrder를 저장하고 있지 않다면 현재 값을 원본으로 저장
                if (!_originalSortingOrderMap.ContainsKey(item))
                {
                    _originalSortingOrderMap[item] = item.sortingOrder;
                }

                // 원본 값에 플레이어 오프셋을 더해서 설정
                item.sortingOrder = _originalSortingOrderMap[item] + playerOrderInLayerPlus * 100;
            }
        }
    }

    #endregion

    #region 색상 / 펄스 / 복원

    private void SetSpriteRendererWhite()
    {
        if (_playerStat != null && _playerStat.MySpriteREndererList != null)
        {
            foreach (var renderer in _playerStat.MySpriteREndererList)
            {
                if (renderer == null) { continue; }
                renderer.color = Color.white;
            }
        }

        if (_player != null && _player.DieSpriteRendererList != null)
        {
            foreach (var renderer in _player.DieSpriteRendererList)
            {
                if (renderer == null) { continue; }
                renderer.color = Color.white;
            }
        }

        _isColorRestored = false; // 색상이 변경됨
    }

    /// <summary>
    /// 경고 펄스 효과 재생 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void PlayPreExplosionPulse()
    {
        if (_preExplosionPulseTween != null && _preExplosionPulseTween.IsActive())
        {
            return;
        }

        float targetScaleMultiplier = PULSE_SCALE_MULTIPLIER;
        float halfDuration = PULSE_HALF_DURATION; // 커졌다/작아졌다 왕복 0.4초
        transform.localScale = _defaultLocalScale;

        Sequence seq = DOTween.Sequence();
        // 커질 때 빨강으로 (원본 색상 기반)
        seq.AppendCallback(() =>
        {
            if (_originalColorMap != null)
            {
                foreach (var kv in _originalColorMap)
                {
                    if (kv.Key == null) { continue; }
                    Color originalColor = kv.Value; // 원본 색상 참조 (읽기 전용)
                    Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                    Color redCol = Color.HSVToRGB(0f, MAX_RED_SATURATION, v);
                    redCol.a = originalColor.a;
                    kv.Key.color = redCol; // 스프라이트 색상만 변경
                }
                _isColorRestored = false; // 색상이 변경됨
            }
        });
        seq.Append(transform.DOScale(_defaultLocalScale * targetScaleMultiplier, halfDuration).SetEase(Ease.InOutSine));
        // 작아질 때 원본 색으로 복구
        seq.AppendCallback(() =>
        {
            RestoreOriginalColors();
        });
        seq.Append(transform.DOScale(_defaultLocalScale, halfDuration).SetEase(Ease.InOutSine));
        seq.SetLoops(-1, LoopType.Restart);

        // DOTween이 중단될 때도 색상 복원
        seq.OnKill(() =>
        {
            RestoreOriginalColors();
        });

        _preExplosionPulseTween = seq;
    }

    /// <summary>
    /// 경고 펄스 효과 중단 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void StopPreExplosionPulse(bool resetScale)
    {
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
            // OnKill 콜백에서 이미 RestoreOriginalColors가 호출됨
        }
        else
        {
            // Tween이 없었다면 색상이 이미 복원된 상태거나 복원이 필요한 상태
            // 안전을 위해 한 번 더 복원 (중복 호출이지만 한 번만 실행됨)
            RestoreOriginalColors();
        }

        if (resetScale)
        {
            transform.localScale = _defaultLocalScale;
        }
    }

    /// <summary>
    /// 저장된 원본 색상으로 스프라이트를 복구합니다.
    /// 모든 색상 효과가 끝날 때 반드시 이 메서드를 호출하여 원본 색상으로 돌아갑니다.
    /// </summary>
    public void RestoreOriginalColors()
    {
        // 이미 복원된 상태라면 중복 실행 방지
        if (_isColorRestored)
        {
            return;
        }

        if (_originalColorMap != null && _originalColorMap.Count > 0)
        {
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key != null)
                {
                    // 원본 색상으로 복원
                    kv.Key.color = kv.Value;
                }
            }

            _isColorRestored = true;
        }
        else
        {
            // 원본 색상 정보가 없는 경우 흰색으로 설정 (비상 조치)
            Debug.LogWarning("[PlayerVisualController] 원본 색상 정보가 없습니다. 흰색으로 복원합니다.");
            SetSpriteRendererWhite();
            _isColorRestored = true;
        }
    }

    /// <summary>
    /// 경고 색상 업데이트 (PlayerGunpowderController에서 호출)
    /// </summary>
    public void UpdateWarningColor(float targetSaturation)
    {
        if (_originalColorMap != null)
        {
            foreach (var kv in _originalColorMap)
            {
                if (kv.Key == null) { continue; }
                Color originalColor = kv.Value; // 원본 색상 참조 (읽기 전용)
                Color.RGBToHSV(originalColor, out float _, out float _, out float v);
                Color newColor = Color.HSVToRGB(0f, targetSaturation, v);
                newColor.a = originalColor.a;
                kv.Key.color = newColor; // 스프라이트 색상만 변경
            }
            _isColorRestored = false; // 색상이 변경됨
        }
    }

    /// <summary>
    /// 펄스 효과가 활성화되어 있는지 확인
    /// </summary>
    public bool IsPreExplosionPulseActive()
    {
        return _preExplosionPulseTween != null && _preExplosionPulseTween.IsActive();
    }

    /// <summary>
    /// Player.OnDisable에서 호출: Tween 정리 및 색상 복원 처리
    /// </summary>
    public void OnOwnerDisable()
    {
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
        }
        else
        {
            // Tween이 없는 경우에만 직접 복원
            RestoreOriginalColors();
        }
    }

    /// <summary>
    /// Player.OnDestroy에서 호출: Tween 정리
    /// </summary>
    public void OnOwnerDestroy()
    {
        if (_preExplosionPulseTween != null)
        {
            _preExplosionPulseTween.Kill(false);
            _preExplosionPulseTween = null;
        }
    }

    #endregion
}

