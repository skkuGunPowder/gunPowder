using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoundSlot : MonoBehaviour
{
    [Header("캐릭터 슬롯")] 
    [SerializeField] private HorizontalLayoutGroup _playerHorizontalLayoutGroup;
    [SerializeField] private GameObject _palyerPivot;
    [SerializeField] private List<CharacterExplosionProduction> _playerList = new List<CharacterExplosionProduction>();
    [SerializeField] private float _fallInterval;
    
    [Header("텍스트 슬롯")]
    [SerializeField] private ShakePower _shakePower = new ShakePower();
    [SerializeField] private RectTransform _slotRectTransform;
    [SerializeField] private UI_TextSlot _textSlot;
    [SerializeField] private float _shakeInterval;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _scale;
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _scaleoffDuration;
    [SerializeField] private float _interval;
    [SerializeField] private float _duration;
    [SerializeField] private Ease _easeType;

    [SerializeField] private RectTransform _backGroundRectTransform;
    [SerializeField] private bool _isNegative = false;
    private List<CharacterExplosionProduction> _activeList = new List<CharacterExplosionProduction>();
    private RectTransform _rectTransform;
    private float _startHeight = 0;
    private bool _scoreChange = false;
    private int _score = 0;
    private int _teamCount = 0;
    
    public int GetTeamCount()
    {
        return _teamCount;
    }
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_slotRectTransform == null)
        {
            _slotRectTransform = _textSlot.GetComponent<RectTransform>();
        }
    }
    
    public void Init(Color32 color, int i, bool negative)
    {
        // 비활성 오브젝트는 null 체크 후 초기화
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        _isNegative = negative;
        
        if (_isNegative)
        {
            _startHeight = - _rectTransform.rect.height;   
        }
        else
        {
            _startHeight = _rectTransform.rect.height;
        }
        Debug.Log($"{_startHeight} is StartHeight");
        // 초기화
        _backGroundRectTransform.anchoredPosition = new Vector2(0, _startHeight);
        
        // 뒷배경 바꾸기
        _textSlot.gameObject.SetActive(true);
        _textSlot.BackgroundRefresh(color);
        TextRefresh(0);
        _playerList[i].gameObject.SetActive(true);
    }

    public void SetComplete()
    {
        foreach (CharacterExplosionProduction player in _playerList)
        {
            if (player.gameObject.activeSelf)
            {
                player.Init();
            }
        }
    }

    public void SetTeamCount(int playerNumber)
    {
        _playerList[playerNumber].gameObject.SetActive(true);
        _teamCount += 1;
    }
    public void TextRefresh(int text)
    {
        _textSlot.TextRefresh(text);
    }

    // 스코어 시작 애니메이션
    public void PlayStart(float duration, Ease ease, Action endCallback = null )
    {
        _duration = duration;
        _easeType = ease;
        _textSlot.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(_backGroundRectTransform.DOAnchorPos(new Vector2(0, 0), duration).SetEase(ease));
        seq.AppendCallback(() => PlayFall(endCallback));
    }

    public void PlayFall(Action endCallback = null)
    {
        _textSlot.gameObject.SetActive(true);
        _palyerPivot.SetActive(true);
        SetComplete();
        PlayPlayerFall(endCallback);
    }
    
    private async UniTaskVoid PlayPlayerFall(Action endCallback = null)
    {
        // 콜백을 마지막 활성 플레이어에게만 등록하기 위해 미리 탐색
        int lastActiveIndex = -1;
        if (endCallback != null)
        {
            for (int i = 0; i < _playerList.Count; i++)
            {
                if (_playerList[i].gameObject.activeSelf)
                {
                    lastActiveIndex = i;
                }
            }
        }

        // 떨어지기 플레이
        for (int i = 0; i < _playerList.Count; i++)
        {
            if (_playerList[i].gameObject.activeSelf)
            {
                if (i == lastActiveIndex)
                {
                    _playerList[i].OnFallEnd += endCallback;
                }

                _playerList[i].PlayFall();
            }

            await UniTask.WaitForSeconds(_fallInterval);
        }

    }

    public void ScoreChange(int text)
    {
        _scoreChange =  true;
        _score = text;
    }
    
    // 점수 변경용 애니메이션
    public void ScorePlay(Action endCallback = null)
    {
     
        Vector2 origin = _slotRectTransform.anchoredPosition;

        if (_scoreChange == false)
        {
            // 우승이 아닌팀은 대기
            Debug.Log("defeat");
            float waitTime = _shakeInterval + _shakeDuration + _scaleoffDuration + _scaleDuration + _interval;
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(waitTime);
            seq.AppendCallback(() => Explosion(_scoreChange));
            return;
        }
        
        Debug.Log("winner");
        // 우승팀은 점수 변경
        Sequence  mySequence = DOTween.Sequence();
        mySequence.AppendInterval(_shakeInterval);
        mySequence.Append(_slotRectTransform.DOShakeAnchorPos(_shakeDuration, strength: _shakePower.VibratePower, vibrato:_shakePower.Vibrato )).SetEase(_shakePower.ShakeEase); // 쉐이크
        mySequence.Join(_slotRectTransform.DOScale(_scale,_scaleDuration));
        // mySequence.Join(_slotRectTransform.DOAnchorPos(origin, _scaleDuration));
        mySequence.JoinCallback(() => TextRefresh(_score));
        mySequence.AppendInterval(_interval);
        mySequence.Append(_slotRectTransform.DOScale(1, _scaleoffDuration)).OnComplete(() =>
        {
            Explosion(_scoreChange);
            endCallback?.Invoke();
        });
    }

    private void Explosion(bool isWinner)
    {
        foreach (CharacterExplosionProduction player in _playerList)
        {
            if (player.gameObject.activeSelf)
            {
                player.PlayExplosion(isWinner);
            }
        }
    }

    public void ScoreStop(Action  endCallback = null)
    {
        Sequence mySequence = DOTween.Sequence();
        // mySequence.AppendCallback(()=> _textSlot.gameObject.SetActive(false));
        mySequence.Append(_backGroundRectTransform.DOAnchorPos(new Vector2(0, _startHeight), _duration).SetEase(_easeType));
        mySequence.AppendCallback(()=> endCallback?.Invoke());
    }

    private void OnDisable()
    {
        // _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, 0f);
        _scoreChange =  false;
        _textSlot.gameObject.SetActive(false);
    }
}
