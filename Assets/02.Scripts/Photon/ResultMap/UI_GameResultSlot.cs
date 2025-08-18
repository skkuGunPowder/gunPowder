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
    
    public ProfileSkin ProfileSkin;
    public void Refresh(PhotonPlayer player,int damage, int rank, int surviveTime, int kill, EInGameTeam team)
    {
        PlayerName.text = player.NickName;
        PlayerRank.text = $"#{rank.ToString()}";
        DamageTextMeshProUGUI.text = damage.ToString();
        KillTextMeshProUGUI.text = kill.ToString();
        SurvivorTimeTextMeshProUGUI.text = TimeSpan.FromSeconds(surviveTime).ToString(@"mm\:ss");
        
        ProfileSkin.Init(player);
        ProfileSkin.TeamChanged(team);
        
        Color color = new Color();
        switch (team)
        {
            case EInGameTeam.Blue : color = new Color32(0, 112, 192,255);
                break;
            case EInGameTeam.Red : color = new Color32(255, 71, 91,255);
                break;
            case EInGameTeam.Green : color = new Color32(93,182, 1,255);
                break;
            case EInGameTeam.Yellow : color = new Color32(255, 228,42,255);
                break;
            default: color = new Color(1, 1, 1);
                break;
        }
        
        TeamColor.color = color;
    }
}
