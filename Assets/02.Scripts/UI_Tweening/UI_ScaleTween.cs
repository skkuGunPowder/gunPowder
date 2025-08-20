using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UI_ScaleTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] private float scaleMultiplier = 1.03f;
	[SerializeField] private float duration = 0.15f;
	[SerializeField] private Ease easing = Ease.OutSine;
	[SerializeField] private bool useUnscaledTime = true;

	private RectTransform rectTransform;
	private Vector3 originalScale;
	private Tween scaleTween;
	private bool isPointerOver;
	private bool isPointerDown;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		originalScale = rectTransform.localScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isPointerOver = true;
		if (!isPointerDown)
		{
			PlayScaleTween(originalScale * scaleMultiplier);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isPointerOver = false;
		PlayScaleTween(originalScale);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isPointerDown = true;
		PlayScaleTween(originalScale);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPointerDown = false;
		if (isPointerOver)
		{
			PlayScaleTween(originalScale * scaleMultiplier);
		}
		else
		{
			PlayScaleTween(originalScale);
		}
	}

	private void PlayScaleTween(Vector3 targetScale)
	{
		if (scaleTween != null && scaleTween.IsActive())
		{
			scaleTween.Kill(false);
		}

		scaleTween = rectTransform.DOScale(targetScale, duration)
			.SetEase(easing)
			.SetUpdate(useUnscaledTime);
	}

	private void OnDisable()
	{
		if (scaleTween != null && scaleTween.IsActive())
		{
			scaleTween.Kill(false);
		}
		rectTransform.localScale = originalScale;
		isPointerOver = false;
		isPointerDown = false;
	}
}
