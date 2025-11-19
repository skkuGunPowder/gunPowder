using UnityEngine;
using System.Collections;
using Microlight.MicroBar;
using Photon.Pun;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private MicroBar healthBar;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("설정")]
    [SerializeField] private float displayDuration = 1.5f;  // 표시 시간
    [SerializeField] private float fadeOutDuration = 0.5f;  // 페이드아웃 시간

    private Player player;
    private PlayerStat playerStat;
    private Coroutine hideCoroutine;
    private bool isInitialized = false;

    void Start()
    {
        // Player와 PlayerStat 참조 가져오기
        player = GetComponentInParent<Player>();
        if (player == null)
        {
            Debug.LogError("[PlayerHealthBar] Player 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        playerStat = player.PlayerStat;
        if (playerStat == null)
        {
            Debug.LogError("[PlayerHealthBar] PlayerStat 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // CanvasGroup 확인 (없으면 추가)
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // MicroBar 초기화
        if (healthBar != null)
        {
            float maxHP = playerStat.InitGunpowderCount;
            healthBar.Initialize(maxHP);
            isInitialized = true;
        }
        else
        {
            Debug.LogError("[PlayerHealthBar] MicroBar가 할당되지 않았습니다!");
        }

        // 건파우더 변경 이벤트 구독
        playerStat.OnGunPowderChanged += OnGunPowderChanged;

        // 처음에는 숨김
        canvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerStat != null)
        {
            playerStat.OnGunPowderChanged -= OnGunPowderChanged;
        }
    }

    /// <summary>
    /// 건파우더가 변경될 때 호출되는 메서드
    /// </summary>
    private void OnGunPowderChanged(int newGunPowder)
    {
        if (!isInitialized || healthBar == null)
        {
            return;
        }

        // 현재 건파우더가 MicroBar의 MaxValue를 넘으면 MaxValue 증가
        if (newGunPowder > healthBar.MaxValue)
        {
            healthBar.SetNewMaxHP(newGunPowder, skipAnimation: true);
        }

        // HP바 업데이트 (증가/감소 자동 감지)
        UpdateAnim animType = newGunPowder > healthBar.CurrentValue ? UpdateAnim.Heal : UpdateAnim.Damage;
        healthBar.UpdateBar(newGunPowder, animType);

        // 표시 및 페이드아웃 코루틴 시작
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(ShowAndHideRoutine());
    }

    /// <summary>
    /// HP바를 표시하고 일정 시간 후 페이드아웃
    /// </summary>
    private IEnumerator ShowAndHideRoutine()
    {
        // 즉시 표시
        canvasGroup.alpha = 1f;

        // 1.5초 대기
        yield return new WaitForSeconds(displayDuration);

        // 0.5초 동안 페이드아웃
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        // 완전히 숨김
        canvasGroup.alpha = 0f;
        hideCoroutine = null;
    }

    /// <summary>
    /// 공격자에게 HP바를 표시 (Player.cs의 RPC에서 호출됨)
    /// </summary>
    public void ShowHealthBarForAttacker(float duration = 3f)
    {
        if (!isInitialized || healthBar == null)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(ShowAndHideRoutine());
    }
}
