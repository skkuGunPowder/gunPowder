using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class AirDropItemLootVFX : MonoBehaviour
{
    public List<Sprite> ItemSpriteList;
    public ParticleSystem _rewardParticle;
    public AudioClip LootSound;
    public AudioClip LootEndSound;

    public GameObject Icon;
    private SpriteRenderer _spriteRenderer;

    private int _lastIndex = -1;
    private Coroutine _rouletteCoroutine;
    private Tween _rotateTween;
    private Tween _scaleTween;

    public bool IsSelected { get; private set; }

    private void OnEnable()
    {
        _spriteRenderer = Icon.GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = ItemSpriteList[UnityEngine.Random.Range(0, ItemSpriteList.Count)];
    }

    public void StartRoulette(AirDropItemBase item)
    {
        Icon.SetActive(true);
        _rouletteCoroutine = StartCoroutine(RouletteRoutine(item));
    }

    public void UseItem()
    {
        if (_rouletteCoroutine != null)
        {
            StopCoroutine(_rouletteCoroutine);
            _rouletteCoroutine = null;
        }

        if (_rotateTween != null) _rotateTween.Kill();
        if (_scaleTween != null) _scaleTween.Kill();

        Icon.transform.localRotation = Quaternion.identity;
        Icon.transform.localScale = Vector3.one;

        _lastIndex = -1;
        IsSelected = false;
        Icon.SetActive(false);
    }

    private IEnumerator RouletteRoutine(AirDropItemBase item)
    {
        float totalTime = 0f;
        float interval = 0.15f;
        float pitch = 1f;

        while (totalTime < 1.2f)
        {
            int newIndex = UnityEngine.Random.Range(0, ItemSpriteList.Count);
            if (newIndex == _lastIndex)
            {
                newIndex = (newIndex + 1) % ItemSpriteList.Count;
            }
            _lastIndex = newIndex;

            _spriteRenderer.sprite = ItemSpriteList[newIndex];

            pitch += 0.2f;
            Sound sound = SoundManager.Instance.PlayGlobalSound(LootSound.name);
            AudioSource audioSource = sound.GetAudioSource();
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }

            yield return new WaitForSeconds(interval);
            totalTime += interval;
            interval *= 1.2f;
        }
        _spriteRenderer.sprite = item.Icon;
        SoundManager.Instance.PlayGlobalSound(LootEndSound.name);

        _rewardParticle.Play();
        IsSelected = true;

        PlayFinalItemAnimation(Icon.transform);
    }

    private void PlayFinalItemAnimation(Transform icon)
    {
        _rotateTween = icon.DOLocalRotate(new Vector3(0, 0, 5f), 0.5f)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);

        _scaleTween = icon.DOScale(1.2f, 0.6f)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
