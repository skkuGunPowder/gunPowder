using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Photon.Pun;

public class FireTruck : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonTransform;
    [SerializeField] private Collider2D damageArea;
    [SerializeField] private SpriteRenderer truckRenderer;
    [SerializeField] private ParticleSystem waterEffect;
    

    [Header("Settings")]
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float deployTime = 1f;
    [SerializeField] private float activeTime = 5f;
    [SerializeField] private float damageInterval = 0.3f;
    [SerializeField] private int damageAmount = 10; // 물풍선 피해량
    [SerializeField] private Vector2 range = new Vector2(15f, 8f);

    private Player _player;
    private Vector3 _spawnPosition;
    private List<IDamagable> targetsInRange = new List<IDamagable>();
    private Coroutine damageCoroutine;

    private void Awake()
    {
        if (damageArea is BoxCollider2D box)
        {
            box.size = range;
            box.isTrigger = true;
        }

        truckRenderer.color = new Color(1, 1, 1, 0); // 투명 시작
        cannonTransform.localScale = Vector3.zero;
        damageArea.enabled = false;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _spawnPosition = _player.transform.position;
    }

    public void Summon()
    {
        transform.position = _spawnPosition + new Vector3(4f, 0f, 0f);

        Sequence seq = DOTween.Sequence();

        // 1. FadeIn
        seq.Append(truckRenderer.DOFade(1f, fadeInTime)); // 페이드 인
        seq.Join(transform.DOMove(_spawnPosition, fadeInTime).SetEase(Ease.OutQuad)); // 앞으로 이동

        // 2. 대포 꺼내기
        seq.AppendCallback(() =>
        {
            cannonTransform.DOScale(Vector3.one, deployTime).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                waterEffect.Play();

                // 3. 발사 시작
                damageArea.enabled = true;
                damageCoroutine = StartCoroutine(DamageOverTime());
                Debug.LogError("공격 중지 예정");
                // 4. activeTime 후 종료
                DOVirtual.DelayedCall(activeTime, StopFireTruck);
            });
        });
    }

    private void StopFireTruck()
    {
        Debug.LogError("공격 중지");

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        waterEffect.Stop();

        damageArea.enabled = false;


        // 대포 접기 & 소방차 사라지기
        cannonTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
        .OnComplete(() =>
        {           
            Sequence seq = DOTween.Sequence();
            seq.Append(truckRenderer.DOFade(0f, 0.5f));
            seq.Join(transform.DOMove(transform.position  + new Vector3(4f, 0, 0), fadeInTime).SetEase(Ease.OutQuad)).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });

        targetsInRange.Clear();
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            foreach (var target in targetsInRange)
            {
                target.TakeDamage(3, transform.position, _player.PhotonView.ViewID, _player.PhotonView.OwnerActorNr);
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable dmg))
        {
            if (!targetsInRange.Contains(dmg))
            {
                targetsInRange.Add(dmg);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable dmg))
        {
            if (targetsInRange.Contains(dmg))
            {
                targetsInRange.Remove(dmg);
            }
        }
    }
} 
