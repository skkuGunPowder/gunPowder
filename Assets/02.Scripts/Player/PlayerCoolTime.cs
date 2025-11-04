using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCoolTime : MonoBehaviour
{
    private Player _myPlayer;

    private Image _normalCoolTimeShadowImage;
    private Image _specialCoolTimeShadowImage;

    private Coroutine _normalCoolTimeCoroutine;
    private Coroutine _specialCoolTimeCoroutine;

    private void Awake()
    {
        _myPlayer = GetComponent<Player>();
        
        // 로컬 플레이어만 UI를 찾음 (멀티플레이어 환경 대응)
        if (_myPlayer.PhotonView.IsMine)
        {
            GameObject normalShadow = GameObject.FindWithTag("NormalCoolTimeShadow");
            if (normalShadow != null)
                _normalCoolTimeShadowImage = normalShadow.GetComponent<Image>();

            GameObject specialShadow = GameObject.FindWithTag("SpecialCoolTimeShadow");
            if (specialShadow != null)
                _specialCoolTimeShadowImage = specialShadow.GetComponent<Image>();
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
            if (_normalCoolTimeCoroutine != null)
            {
                StopCoroutine(_normalCoolTimeCoroutine);
            }
            Debug.Log($"SetNormalAttackCoolTime: {_myPlayer.BasicBombStat.CoolTime}초");
            _normalCoolTimeCoroutine = StartCoroutine(CoolTimeCoroutine(_normalCoolTimeShadowImage, _myPlayer.BasicBombStat.CoolTime));
        }
    }

    private void SetSpecialAttackCoolTime()
    {
        // UI가 있는 씬에서만 실행
        if (_specialCoolTimeShadowImage != null && _myPlayer.SpecialBombStat != null)
        {
            if (_specialCoolTimeCoroutine != null)
                StopCoroutine(_specialCoolTimeCoroutine);

            Debug.Log($"SetSpecialAttackCoolTime: {_myPlayer.SpecialBombStat.CoolTime}초");
            _specialCoolTimeCoroutine = StartCoroutine(CoolTimeCoroutine(_specialCoolTimeShadowImage, _myPlayer.SpecialBombStat.CoolTime));
        }
    }

    private IEnumerator CoolTimeCoroutine(Image shadowImage, float coolTime)
    {
        float elapsed = 0f;
        shadowImage.fillAmount = 1f; // 쿨타임 시작 (가득 참)

        while (elapsed < coolTime)
        {
            elapsed += Time.deltaTime;
            shadowImage.fillAmount = 1f - (elapsed / coolTime); // 서서히 비워짐
            yield return null;
        }

        shadowImage.fillAmount = 0f; // 쿨타임 완료
    }
    
    private void OnDestroy()
    {
        if (_myPlayer != null)
        {
            _myPlayer.OnNormalAttack -= SetNormalAttackCoolTime;
            _myPlayer.OnSpecialAttack -= SetSpecialAttackCoolTime;
        }
    }
}
