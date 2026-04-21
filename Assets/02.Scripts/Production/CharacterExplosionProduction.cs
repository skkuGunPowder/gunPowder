using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterExplosionProduction : MonoBehaviour
{
    private RectTransform _rectTransform;
 
    // 현재 위치 첫 위치에 떨어지기
    [SerializeField] private float _fallSpeed;
    [SerializeField] private float _height;
    [SerializeField] private float _down;
    [SerializeField] private Vector2 _fallStartVector;
    [SerializeField] private Vector2 _fallEndVector;
    [SerializeField] private Vector2 _awayVector;
    [SerializeField] private Ease _fallEase = Ease.OutBounce;
    
    //날아갈 위치와 스케일
    [SerializeField] private float _backScale;
    [SerializeField] private float _backSpeed;
    [SerializeField] private float _backTime;
    
    [SerializeField] private float _forwardScale;
    [SerializeField] private float _forwardSpeed;
    [SerializeField] private float _forwardTime;

    [SerializeField] private float _maxDown;
    [SerializeField] private float _maxUp;

    public event Action OnFallEnd;

    private void Awake()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }

    public void Init()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        _down = _rectTransform.rect.y;
        _rectTransform.position = new Vector2(_rectTransform.rect.x, _rectTransform.rect.y + _height);
        Debug.Log("current rectTransform" + _rectTransform.position);
    }
    
    
    public void PlayFall()
    {
        Init();
        Sequence seq = DOTween.Sequence();
        // seq.AppendInterval(_fallSpeed);
        // seq.OnComplete(() =>
        // {
        //     OnFallEnd?.Invoke();
        // });
        _rectTransform.DOLocalMove(new Vector2(_rectTransform.rect.x , _down), _fallSpeed).SetEase(_fallEase).OnComplete(() =>
        {
            OnFallEnd?.Invoke();
        });
    }

    public void PlayExplosion()
    {
        
    }
    
    // 뒤로 날아가기 (scale --)
    public void ExplosionBackward()
    {
    }

    // 앞으로 날아가기 (scale ++)
    public void ExplosionForward()
    {
        
    }

    private void OnDisable()
    {
        // 초기화 (scale 초기화)
        _rectTransform.localScale = new Vector3(1f,1f,1f);
    }
}
