using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
public class UI_KillPannel : MonoBehaviour
{
    public TextMeshProUGUI KillPannelText;
    public TextMeshProUGUI KillPannelBG;
    [SerializeField] private float _fadeTime = 1f;
    [SerializeField] private float _intervalTime = 1f;
    private Color32 _bgColor;
    private Color32 _textColor;
    
    private void Awake()
    {
        _bgColor = KillPannelBG.color;
        _textColor = KillPannelText.color;
    }
    public void Refresh(string playerNickname)
    {
        
        DOTween.Kill(this);
        
        // 중간에 킬 되었을 때, 처음 시작
        KillPannelBG.text = $"{playerNickname}를 처치하였습니다.";
        KillPannelText.text = $"<color=red>{playerNickname}</color>를 처치하였습니다.";
        
        KillPannelBG.color = _bgColor;
        KillPannelText.color = _textColor;
        
        Play();
    }

    private void Play()
    {
        // 페이드 아웃
        SoundManager.Instance.PlayLocalSound("KillLocal_2", transform);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(_intervalTime);
        sequence.Append(KillPannelBG.DOFade(0, _fadeTime));
        sequence.Join(KillPannelText.DOFade(0, _fadeTime));
        
        
        
    }

    private void OnDisable()
    {
        KillPannelBG.color = _bgColor;
        KillPannelText.color = _textColor;
    }

}
