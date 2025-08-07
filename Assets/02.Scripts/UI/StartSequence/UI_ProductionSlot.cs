using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_ProductionSlot : MonoBehaviour
{
    // 아이디
    public TextMeshProUGUI PlayerNicknameText;
    // 현재 로딩이 완료되었는가?
    public GameObject LoadingImage;
    // 프로필 이미지
    public Image PlayerProfileImage;
    // 팀별 색
    public List<Color32> TeamColorCodeList;
    // 현재 장착중인 무기
    public Image Bomb;
    public RectTransform BombObject;
    public RectTransform Shine;
    public Image Glow;
    
    // 함수 = Init (로딩 제외)
    public void Init(string nickname, EInGameTeam team, Sprite bombImage)
    {
        PlayerNicknameText.text = nickname;
        Bomb.sprite = bombImage;
        PlayerProfileImage.color = TeamColorSet(team);
    }

    public void LoadCheck(bool isLoad)
    {
        LoadingImage.SetActive(!isLoad);
    }
    private Color32 TeamColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red:
                return TeamColorCodeList[0];
            case EInGameTeam.Blue:
                return TeamColorCodeList[1];
            case EInGameTeam.Green:
                return TeamColorCodeList[2];
            case EInGameTeam.Yellow:
                return TeamColorCodeList[3];
            case EInGameTeam.Default:
                return TeamColorCodeList[4];
            default:
                return TeamColorCodeList[0];
        }
    }

    public void ShineOn()
    {
        Shine.DOAnchorPos(new Vector2(800, 0), 1f);
    }

    public void FlashOn()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(Glow.DOFade(0.4f, 0.2f));
        sequence.InsertCallback(0.1f, BombOn);
        sequence.Append(Glow.DOFade(0, 0.2f));
    }

    private void BombOn()
    {
        BombObject.gameObject.SetActive(true);
        BombObject.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBack);
    }
    public void Shake()
    {
        gameObject.transform.DOShakePosition(0.5f, 10f, 20, 90, false, true);
    }
    
    private void OnDisable()
    {
        DOTween.KillAll();
        Shine.anchoredPosition = new Vector2(-700, 0);
        Glow.color = new Color(1,1,1, 0);
        BombObject.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        BombObject.gameObject.SetActive(false);
    }   
}
