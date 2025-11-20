using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Microlight.MicroBar;
using Photon.Pun;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private MicroBar healthBar;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image hpBarImage;        // Inspector에서 할당 (HPBar Image)
    [SerializeField] private Image hpGhostBarImage;   // Inspector에서 할당 (HPGhostBar Image)

    [Header("설정")]
    [SerializeField] private float displayDuration = 1.5f;  // 표시 시간
    [SerializeField] private float fadeOutDuration = 0.5f;  // 페이드아웃 시간

    [Header("자신의 HP bar 색상")]
    [SerializeField] private Color selfHPBarColor = new Color(0.2f, 1f, 0.48f);  // 33FF7B (자신)
    [SerializeField] private Color selfGhostBarColor = new Color(1f, 0.2f, 0.2f);  // FF3333 (자신)

    private Player player;
    private PlayerStat playerStat;
    private Coroutine hideCoroutine;
    private bool isInitialized = false;

    private int maxHP;
    private int CurrentHP;

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
            maxHP = playerStat.InitGunpowderCount;
            healthBar.Initialize(maxHP);
            isInitialized = true;

            // 자신의 플레이어면 색상 변경 (적 색상은 Inspector에서 기본으로 설정)
            if (player.PhotonView.IsMine)
            {
                SetSelfHealthBarColors();
            }
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
    /// 자신의 HP bar 색상 설정
    /// </summary>
    private void SetSelfHealthBarColors()
    {
        if (hpBarImage != null)
        {
            hpBarImage.color = selfHPBarColor;
        }

        if (hpGhostBarImage != null)
        {
            hpGhostBarImage.color = selfGhostBarColor;
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
            maxHP = newGunPowder;
            healthBar.SetNewMaxHP(maxHP, skipAnimation: true);
        }
        CurrentHP = newGunPowder;

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
    /// 부활 시 HP bar를 초기 상태로 리셋 (Player.cs의 ResurrectPlayer에서 호출됨)
    /// </summary>
    public void ResetHealthBarOnResurrect()
    {
        if (!isInitialized || healthBar == null || playerStat == null)
        {
            return;
        }

        // maxHP를 초기값으로 리셋
        maxHP = playerStat.InitGunpowderCount;
        healthBar.SetNewMaxHP(maxHP, skipAnimation: true);
        
        // 현재 HP도 초기값으로 설정
        int currentHP = playerStat.CurrentPlayerGunPowderCount;
        healthBar.UpdateBar(currentHP, skipAnimation: true);
        
        // HP bar 숨김
        canvasGroup.alpha = 0f;
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    /// <summary>
    /// 공격자에게 HP바를 표시 (Player.cs의 RPC에서 호출됨)
    /// </summary>
    /// <param name="currentHP">피격자의 현재 HP (데미지 받은 후)</param>
    /// <param name="damage">받은 데미지</param>
    /// <param name="isAfterResurrect">부활 후 첫 공격 여부</param>
    public void ShowHealthBarForAttacker(int currentHP, int damage, bool isAfterResurrect)
    {
        if (!isInitialized || healthBar == null)
        {
            return;
        }
        
        // 데미지 받기 전 HP
        int hpBeforeDamage = currentHP + damage;
        
        // 부활 후 첫 공격이면 maxHP를 리셋
        if (isAfterResurrect)
        {
            maxHP = hpBeforeDamage;
        }
        // 그렇지 않고 HP가 증가했으면 (회복) maxHP 업데이트
        else if (hpBeforeDamage > maxHP)
        {
            maxHP = hpBeforeDamage;
        }

        // 파라미터로 받은 정확한 값 사용
        healthBar.SetNewMaxHP(maxHP, skipAnimation: true);
        healthBar.UpdateBar(hpBeforeDamage, skipAnimation: true);  // 데미지 받기 전
        healthBar.UpdateBar(currentHP, UpdateAnim.Damage);         // 데미지 받은 후

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(ShowAndHideRoutine());
    }
}
