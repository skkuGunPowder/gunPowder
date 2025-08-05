using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    // 로딩이 되었는지 체크하는 칸
}
