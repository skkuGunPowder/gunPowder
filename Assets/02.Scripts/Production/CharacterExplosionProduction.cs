using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterExplosionProduction : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform; // 플레이어 아이콘
 
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
            _rectTransform = GetComponentInChildren<RectTransform>();
        }
    }

    public void Init()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponentInChildren<RectTransform>();
        }

        _rectTransform.anchoredPosition = new Vector2(0, _height);
    }
    
    
    public void PlayFall()
    {
        _rectTransform.gameObject.SetActive(true);
        _rectTransform.DOAnchorPos(new Vector2(0 , _down), _fallSpeed).SetEase(_fallEase).OnComplete(() =>
        {
            OnFallEnd?.Invoke();
        });
    }

    public void PlayExplosion(bool winner)
    {
        if (winner)
        {
            ExplosionBackward();
        }
        else
        {
            ExplosionForward();
        }
    }
    
    // 뒤로 날아가기 (scale --)
    public void ExplosionBackward()
    {
        Debug.Log("backward");
        // _rectTransform.gameObject.SetActive(false);
    }

    // 앞으로 날아가기 (scale ++)
    public void ExplosionForward()
    {
        Debug.Log("forward");
        // _rectTransform.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 초기화 (scale 초기화)
        OnFallEnd = null;
        _rectTransform.gameObject.SetActive(false);
        _rectTransform.localScale = new Vector3(1f,1f,1f);
    }
}
