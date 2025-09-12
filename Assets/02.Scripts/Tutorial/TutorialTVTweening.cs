using UnityEngine;
using DG.Tweening;

public class TutorialTVTweening : MonoBehaviour
{
    [Header("Target (optional)")]
    [SerializeField] private SpriteRenderer _spriteRenderer; // Assign if you want alpha fade

    [Header("Settings")]
    [SerializeField] private float _duration = 0.25f; // total turn-on time
    [SerializeField] private float _overshootScaleY = 1.15f; // slight bounce
    [SerializeField] private float _fadeDuration = 0.1f; // total turn-on time
    [SerializeField] private bool _ignoreTimeScale = true; // play even if timeScale==0

    private Sequence _sequence;
    private Vector3 _originalScale;
    private Color _originalColor = Color.white;

    private void Awake()
    {
        _originalScale = transform.localScale;
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        PlayTurnOn();
    }

    private void OnDisable()
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
        }
        // Ensure final state
        transform.localScale = _originalScale;
        if (_spriteRenderer != null)
        {
            var c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }
    }

    public void PlayTurnOn()
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
        }

        // Start as a thin horizontal line, invisible
        transform.localScale = new Vector3(_originalScale.x, 0.01f, _originalScale.z);
        
        if (_spriteRenderer != null)
        {
            var c = _spriteRenderer.color;
            c.a = 0f;
            _spriteRenderer.color = c;
        }

        float half = Mathf.Max(0.01f, _duration * 0.6f);
        float rest = Mathf.Max(0.01f, _duration - half);

        _sequence = DOTween.Sequence();
        if (_ignoreTimeScale)
        {
            _sequence.SetUpdate(true);
        }

        // Fade should start at the very beginning
        if (_spriteRenderer != null)
        {
            _sequence.Insert(0f, _spriteRenderer.DOFade(1f, _fadeDuration).SetEase(Ease.Linear));
        }

        // Scale up quickly, slight overshoot, then settle
        _sequence.Append(transform.DOScaleY(_overshootScaleY * _originalScale.y, half).SetEase(Ease.OutExpo));
        _sequence.Append(transform.DOScaleY(_originalScale.y, rest).SetEase(Ease.OutBack));

        _sequence.Play();
    }
}
