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
    [SerializeField] private UI_TextSlot _textSlot;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _scale;
    [SerializeField] private float _scaleDuration;
    [SerializeField] private float _interval;
    [SerializeField] private float _duration;
    [SerializeField] private Ease _easeType;

    private List<CharacterExplosionProduction> _activeList = new List<CharacterExplosionProduction>();
    private RectTransform _rectTransform;
    private RectTransform _slotRectTransform;
    private float _startHeight;
    private bool _scoreChange = false;
    private int _score = 0;
    private int _teamCount = 0;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startHeight = _rectTransform.rect.height;
    }
    
    public void Init(Color32 color, int i)
    {
        _textSlot.gameObject.SetActive(true);
        
        // 뒷배경 바꾸기
        _textSlot.BackgroundRefresh(color);
        TextRefresh(0);
        _playerList[i].gameObject.SetActive(true);
    }

    public void SetComplete()
    {
        _playerHorizontalLayoutGroup.enabled = true;
        Debug.Log("horizon : on");
        _playerHorizontalLayoutGroup.enabled = false;
        Debug.Log("horizon : off");
        
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
    public int GetTeamCount()
    {
        return _teamCount;
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
        Sequence seq = DOTween.Sequence();
        seq.Append(_rectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, _startHeight), duration).SetEase(ease));
        seq.AppendCallback(() => _textSlot.gameObject.SetActive(true));
        seq.OnComplete(() =>
        {
            PlayFall(endCallback);
        });
    }

    private void PlayFall(Action endCallback = null)
    {
        _palyerPivot.SetActive(true);
        SetComplete();
        PlayPlayerFall(endCallback);
    }
    
    private async UniTaskVoid PlayPlayerFall(Action endCallback = null)
    {
        for (int i = 0; i < _playerList.Count; i++)
        {
            if (_playerList[i].gameObject.activeSelf)
            {
                if (endCallback != null)
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
        if (_scoreChange == false)
        {
            return;
        }
        
        Sequence  mySequence = DOTween.Sequence();
        mySequence.Append(_slotRectTransform.DOShakeAnchorPos(_shakeDuration)); // 쉐이크
        mySequence.Append(_slotRectTransform.DOScale(_scale,_scaleDuration));
        mySequence.JoinCallback(() => TextRefresh(_score));
        mySequence.AppendInterval(_interval);
        mySequence.Append(_slotRectTransform.DOScale(1,_scaleDuration)).OnComplete(() =>
        {
            endCallback?.Invoke();
        });
    }

    public void ScoreStop(Action  endCallback = null)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.AppendCallback(()=> _textSlot.gameObject.SetActive(false));
        mySequence.Append(_rectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, 0f), _duration).SetEase(_easeType));
        mySequence.AppendCallback(()=> endCallback?.Invoke());
    }

    private void OnDisable()
    {
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, 0f);
        _textSlot.gameObject.SetActive(false);
    }
}
