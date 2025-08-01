using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_KillLogSlot : MonoBehaviour
{
    public TextMeshProUGUI KillPlayerNickname;
    public TextMeshProUGUI DeathPlayerNickname;

    public Image KillIcon;
    [Header("아이콘")]
    public Sprite Icon;
    
    public void Refresh(string kill,string death)
    {
        KillPlayerNickname.text = kill;
        DeathPlayerNickname.text = death;
        KillIcon.sprite = Icon;
    }

    private void UI_Tweening()
    {
        
    }
}
