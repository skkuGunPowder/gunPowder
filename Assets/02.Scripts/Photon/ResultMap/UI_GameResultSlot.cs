using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_GameResultSlot : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI PlayerRank;
    public Image TeamColor;
    public TextMeshProUGUI DamageTextMeshProUGUI;
    public TextMeshProUGUI KillTextMeshProUGUI;
    public TextMeshProUGUI SurvivorTimeTextMeshProUGUI;
    public TextMeshProUGUI GoldTextMeshProUGUI;
    public TextMeshProUGUI EXPTextMeshProUGUI;
    public ProfileSkin ProfileSkin;
    
    [Header("팀별 색상")] 
    public ColorPalette ColorPalette;
    
    public void Refresh(PhotonPlayer player,int damage, int rank, int surviveTime, int kill, EInGameTeam team, int gold, int exp)
    {
        PlayerName.text = player.NickName;
        PlayerRank.text = $"#{rank.ToString()}";
        DamageTextMeshProUGUI.text = damage.ToString();
        KillTextMeshProUGUI.text = kill.ToString();
        SurvivorTimeTextMeshProUGUI.text = TimeSpan.FromSeconds(surviveTime).ToString(@"mm\:ss");
        ProfileSkin.Init(player);
        ProfileSkin.TeamChanged(team);

        GoldTextMeshProUGUI.text = gold.ToString();
        EXPTextMeshProUGUI.text = exp.ToString();
        
        Color color = new Color();
        switch (team)
        {
            case EInGameTeam.Blue :
                color = ColorPalette.ColorDictionary[EColorType.Blue];
                break;
            case EInGameTeam.Red : 
                color = ColorPalette.ColorDictionary[EColorType.Red];
                break;
            case EInGameTeam.Green : 
                color = ColorPalette.ColorDictionary[EColorType.Green];
                break;
            case EInGameTeam.Yellow :
                color = ColorPalette.ColorDictionary[EColorType.Yellow];
                break;
            default: 
                color = ColorPalette.ColorDictionary[EColorType.White];
                break;
        }
        
        TeamColor.color = color;
    }
}
