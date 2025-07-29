using TMPro;
using UnityEngine;

public class UI_GameResultSlot : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI DamageTextMeshProUGUI;
    public TextMeshProUGUI KillTextMeshProUGUI;
    public TextMeshProUGUI SurvivorTimeTextMeshProUGUI;

    public void Refresh(int player,int damage, int surviveTime, int kill)
    {
        PlayerName.text = player.ToString();
        DamageTextMeshProUGUI.text = damage.ToString();
        KillTextMeshProUGUI.text = kill.ToString();
        SurvivorTimeTextMeshProUGUI.text = surviveTime.ToString();
    }
}
