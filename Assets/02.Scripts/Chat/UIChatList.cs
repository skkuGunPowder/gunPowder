using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;

public class UIChatList : MonoBehaviour
{
    public Toggle CheckBox = null;
    public ProfileSkin PlayerProfileSkin = null;
    public TextMeshProUGUI Name = null;
    public TextMeshProUGUI Message = null;
    public TextMeshProUGUI Time = null;
    public Button ReportButton = null;
    public Image OutLineImage = null;

    private UInt64 _index = 0;
    private string _tag = string.Empty;
    private string _gamerName = string.Empty;  // ★ 닉네임 저장

    // ★ 외부에서 이 글이 누구 건지 확인할 수 있게 프로퍼티 추가
    public int ActorNumber => (int)_index;
    public string GamerName => _gamerName;  // ★ 닉네임 프로퍼티
 
    public void SetData(List<string> outfitIds,UInt64 index, string avatar, string nickname, string message, string time, string tag,
        Action<UInt64, string> report, Action<bool, string> translate,
        bool is_my = false, EInGameTeam team = EInGameTeam.Red)
    {
        _index = index;
        _gamerName = nickname;  // ★ 닉네임 저장

        if (PlayerProfileSkin != null)
        {
            PlayerProfileSkin.gameObject.SetActive(true);
            PlayerProfileSkin.Init(outfitIds); 
        }
        if (is_my) Name.text = "[전체]";
        else Name.text = $"[전체] {nickname}";

        Message.text = message;

        // 초기 색상 설정
        UpdateTeamColor(team);
    }
// ★ public으로 변경하여 Popup에서 호출 가능하게 함
    public void UpdateTeamColor(EInGameTeam team)
    {
        if (!OutLineImage) return;
        EColorType targetColorType = EColorType.Red;
        switch (team)
        {
            case EInGameTeam.Red: targetColorType = EColorType.Red; break;
            case EInGameTeam.Blue: targetColorType = EColorType.Blue; break;
            case EInGameTeam.Green: targetColorType = EColorType.Green; break;
            case EInGameTeam.Yellow: targetColorType = EColorType.Yellow; break;
            default: OutLineImage.color = Color.white; return;
        }
        if (ColorPalette.ColorDictionary != null &&
            ColorPalette.ColorDictionary.TryGetValue(targetColorType, out var value))
            OutLineImage.color = value;
        else
            OutLineImage.color = Color.white;
    }
    public bool IsEqual(UInt64 index, string tag) { return _index == index && _tag == tag; }
    public void SetMessage(string message) {Message.text = message;}

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
