using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;

public class UIChatList : MonoBehaviour
{
    public Toggle CheckBox = null;
    public Image Avatar = null;
    public Text Name = null;
    public TextMeshProUGUI Message = null;
    public Text Time = null;
    public Button ReportButton = null;
    public Image OutLineImage = null;

    private UInt64 _index = 0;
    private string _tag = string.Empty;
    
    // ★ 외부에서 이 글이 누구 건지 확인할 수 있게 프로퍼티 추가
    public int ActorNumber => (int)_index;
    
    // 팀 컬러 변경 로직
    // ★ 팀에 따른 색상 변경 로직 (ColorPalette 사용)
    private void SetTeamColor(EInGameTeam team)
    {
        if (OutLineImage == null) return;

        EColorType targetColorType = EColorType.Red;

        // EInGameTeam -> EColorType 변환
        switch (team)
        {
            case EInGameTeam.Red:
                targetColorType = EColorType.Red;
                break;
            case EInGameTeam.Blue:
                targetColorType = EColorType.Blue;
                break;
            case EInGameTeam.Green:
                targetColorType = EColorType.Green;
                break;
            default:
                // 팀이 없거나(None) 개인전일 경우 기본 흰색 혹은 투명
                OutLineImage.color = Color.white; 
                return;
        }

        // ColorPalette에서 색상 가져와 적용
        if (ColorPalette.ColorDictionary != null && 
            ColorPalette.ColorDictionary.ContainsKey(targetColorType))
        {
            OutLineImage.color = ColorPalette.ColorDictionary[targetColorType];
        }
        else
        {
            // 팔레트가 로드되지 않았을 경우를 대비한 기본값
            OutLineImage.color = Color.white;
        }
    }
    public void SetData(UInt64 index, string avatar, string name, string message, string time, string tag, 
        Action<UInt64, string> report, Action<bool, string> translate, 
        bool is_my = false, EInGameTeam team = EInGameTeam.Red)
    {
        _index = index;

        if (string.IsNullOrEmpty(avatar) || avatar == "default")
            Avatar.sprite = Resources.Load<Sprite>("Images/Girl_5");
        else
            Avatar.sprite = Resources.Load<Sprite>("Images/" + avatar);

        if (is_my) Name.text = "";
        else Name.text = name;
        
        Message.text = message;

        // 초기 색상 설정
        UpdateTeamColor(team);
    }
// ★ public으로 변경하여 Popup에서 호출 가능하게 함
    public void UpdateTeamColor(EInGameTeam team)
    {
        if (OutLineImage == null) return;

        EColorType targetColorType = EColorType.Red;

        switch (team)
        {
            case EInGameTeam.Red:
                targetColorType = EColorType.Red;
                break;
            case EInGameTeam.Blue:
                targetColorType = EColorType.Blue;
                break;
            case EInGameTeam.Green:
                targetColorType = EColorType.Green;
                break;
            default:
                OutLineImage.color = Color.white; // 팀 없음/개인전
                return;
        }

        if (ColorPalette.ColorDictionary != null && 
            ColorPalette.ColorDictionary.ContainsKey(targetColorType))
        {
            OutLineImage.color = ColorPalette.ColorDictionary[targetColorType];
        }
        else
        {
            OutLineImage.color = Color.white;
        }
    }
    public bool IsEqual(UInt64 index, string tag)
    {
        return _index == index && _tag == tag;
    }

    public void SetMessage(string message)
    {
        Message.text = message;
    }

    /// <summary>
    /// 시스템 메시지 스타일 적용 (이름과 메시지 텍스트를 노란색으로)
    /// </summary>
    public void ApplySystemMessageStyle()
    {
        if (Name != null) Name.color = Color.yellow;
        if (Message != null) Message.color = Color.yellow;
        if (OutLineImage != null) OutLineImage.color = Color.white; // 시스템은 흰색 테두리
    }
}
