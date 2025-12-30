using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading;
using TMPro;
using Cysharp.Threading.Tasks;

public class PlayerCoolTime : MonoBehaviour
{
    private Player _myPlayer;

    private Image _normalCoolTimeShadowImage;
    private Image _specialCoolTimeShadowImage;
    
    private TextMeshProUGUI _normalCoolTimeText;
    private TextMeshProUGUI _specialCoolTimeText;

    private Image _normalCoolTimeEndEffectImage;
    private Image _specialCoolTimeEndEffectImage;

    private CancellationTokenSource _normalCoolTimeCancellationTokenSource;
    private CancellationTokenSource _specialCoolTimeCancellationTokenSource;
    
    [Header("Special Cool Time")]
    [SerializeField] private GameObject _specialCoolTimeEffectPrefab;
    [SerializeField] private AudioClip _specialCoolTimeEndAudio;

    private void Awake()
    {
        _myPlayer = GetComponent<Player>();

        // 로컬 플레이어만 UI를 찾음 (멀티플레이어 환경 대응)
        if (_myPlayer.PhotonView.IsMine)
        {
            GameObject normalShadow = GameObject.FindWithTag("NormalCoolTimeShadow");
            if (normalShadow != null)
            {
                _normalCoolTimeShadowImage = normalShadow.GetComponent<Image>();

                _normalCoolTimeText = normalShadow.GetComponentInChildren<TextMeshProUGUI>();
                if (_normalCoolTimeText != null)
                {
                    _normalCoolTimeText.text = "";
                }

                // 자식 오브젝트를 이름으로 찾기
                Transform endEffectTransform = normalShadow.transform.Find("BombCooltimeEffect");
                if (endEffectTransform != null)
                {
                    _normalCoolTimeEndEffectImage = endEffectTransform.GetComponent<Image>();
                    // 초기 알파값 0으로 설정
                    if (_normalCoolTimeEndEffectImage != null)
                    {
                        Color color = _normalCoolTimeEndEffectImage.color;
                        color.a = 0f;
                        _normalCoolTimeEndEffectImage.color = color;
                    }
                }
            }


            GameObject specialShadow = GameObject.FindWithTag("SpecialCoolTimeShadow");
            if (specialShadow != null)
            {
                _specialCoolTimeShadowImage = specialShadow.GetComponent<Image>();

                _specialCoolTimeText = specialShadow.GetComponentInChildren<TextMeshProUGUI>();
                if (_specialCoolTimeText != null)
                {
                    _specialCoolTimeText.text = "";
                }

                // 자식 오브젝트를 이름으로 찾기
                Transform endEffectTransform = specialShadow.transform.Find("SpecialBombCooltimeEffect");
                if (endEffectTransform != null)
                {
                    _specialCoolTimeEndEffectImage = endEffectTransform.GetComponent<Image>();
                    // 초기 알파값 0으로 설정
                    if (_specialCoolTimeEndEffectImage != null)
                    {
                        Color color = _specialCoolTimeEndEffectImage.color;
                        color.a = 0f;
                        _specialCoolTimeEndEffectImage.color = color;
                    }
                }
            }
        }
    }

    private void Start()
    {
        _myPlayer.OnNormalAttack += SetNormalAttackCoolTime;
        _myPlayer.OnSpecialAttack += SetSpecialAttackCoolTime;
    }

    private void SetNormalAttackCoolTime()
    {
        if (_normalCoolTimeShadowImage != null && _myPlayer.BasicBombStat != null)
        {
            if (_normalCoolTimeCancellationTokenSource != null)
            {
                _normalCoolTimeCancellationTokenSource.Cancel();
                _normalCoolTimeCancellationTokenSource.Dispose();
            }
            _normalCoolTimeCancellationTokenSource = new CancellationTokenSource();
            CoolTimeCoroutine(_normalCoolTimeShadowImage, _normalCoolTimeText, _normalCoolTimeEndEffectImage, _myPlayer.BasicBombStat.CoolTime, _normalCoolTimeCancellationTokenSource.Token).Forget();
        }
    }

    private void SetSpecialAttackCoolTime()
    {
        // UI가 있는 씬에서만 실행
        if (_specialCoolTimeShadowImage != null && _myPlayer.SpecialBombStat != null)
        {
            if (_specialCoolTimeCancellationTokenSource != null)
            {
                _specialCoolTimeCancellationTokenSource.Cancel();
                _specialCoolTimeCancellationTokenSource.Dispose();
            }
            _specialCoolTimeCancellationTokenSource = new CancellationTokenSource();
            CoolTimeCoroutine(_specialCoolTimeShadowImage, _specialCoolTimeText, _specialCoolTimeEndEffectImage, _myPlayer.SpecialBombStat.CoolTime, _specialCoolTimeCancellationTokenSource.Token).Forget();
        }
    }

    private async UniTask CoolTimeCoroutine(Image shadowImage, TextMeshProUGUI coolTimeText, Image endEffectImage, float coolTime, CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        shadowImage.fillAmount = 1f; // 쿨타임 시작 (가득 참)

        while (elapsed < coolTime && !cancellationToken.IsCancellationRequested)
        {
            elapsed += Time.deltaTime;
            float remainingTime = coolTime - elapsed;
            
            // fillAmount 업데이트
            shadowImage.fillAmount = 1f - (elapsed / coolTime); // 서서히 비워짐
            
            // 텍스트 업데이트
            if (coolTimeText != null)
            {
                if (remainingTime > 1f)
                {
                    // 1초 초과일 때는 정수로 표시 (올림)
                    coolTimeText.text = Mathf.Ceil(remainingTime).ToString("F0");
                }
                else
                {
                    // 1초 이하일 때는 소수점 한자리로 표시
                    coolTimeText.text = remainingTime.ToString("F1");
                }
            }
            
            await UniTask.Yield(cancellationToken: cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            shadowImage.fillAmount = 0f; // 쿨타임 완료
            
            // 쿨타임이 끝나면 텍스트 숨김
            if (coolTimeText != null)
            {
                coolTimeText.text = "";
            }
            
            // 쿨타임 종료 효과 이미지 페이드 아웃
            if (endEffectImage != null)
            {
                FadeOutEffect(endEffectImage, cancellationToken).Forget();
            }
            
            // 스페셜 쿨타임 종료 시 VFX & SFX 프리팹 재생
            if (shadowImage == _specialCoolTimeShadowImage && _specialCoolTimeEffectPrefab != null)
            {
                PlayCoolTimeEndVFX(_specialCoolTimeEffectPrefab);
                SoundManager.Instance.PlayLocalSound(_specialCoolTimeEndAudio.name, _myPlayer.transform, 0, false);
            }
        }
    }
    
    private async UniTask FadeOutEffect(Image effectImage, CancellationToken cancellationToken)
    {
        // 알파값 200/255 = 약 0.784로 설정
        Color color = effectImage.color;
        color.a = 200f / 255f;
        effectImage.color = color;
        
        float fadeTime = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime && !cancellationToken.IsCancellationRequested)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(200f / 255f, 0f, elapsed / fadeTime);
            effectImage.color = color;
            await UniTask.Yield(cancellationToken: cancellationToken);
        }
        
        if (!cancellationToken.IsCancellationRequested)
        {
            // 완전히 투명하게
            color.a = 0f;
            effectImage.color = color;
        }
    }
    
    private void PlayCoolTimeEndVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab != null && VFXPool.Instance != null)
        {
            FollowVFX vfx = VFXPool.Instance.Get(vfxPrefab.name) as FollowVFX;
            if (vfx != null)
            {
                if (_myPlayer != null && _myPlayer.gameObject.activeInHierarchy)
                {
                    vfx.PlayAttached(_myPlayer.transform);
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        if (_myPlayer != null)
        {
            _myPlayer.OnNormalAttack -= SetNormalAttackCoolTime;
            _myPlayer.OnSpecialAttack -= SetSpecialAttackCoolTime;
        }

        // CancellationTokenSource 정리
        if (_normalCoolTimeCancellationTokenSource != null)
        {
            _normalCoolTimeCancellationTokenSource.Cancel();
            _normalCoolTimeCancellationTokenSource.Dispose();
            _normalCoolTimeCancellationTokenSource = null;
        }
        if (_specialCoolTimeCancellationTokenSource != null)
        {
            _specialCoolTimeCancellationTokenSource.Cancel();
            _specialCoolTimeCancellationTokenSource.Dispose();
            _specialCoolTimeCancellationTokenSource = null;
        }
    }
}
