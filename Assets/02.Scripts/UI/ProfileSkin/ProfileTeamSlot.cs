using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileTeamSlot : MonoBehaviour
{
    // r b g y 순
    public List<Sprite> TeamImage;
    public Image TeamSkin;

    private void Awake()
    {
        TeamSkin = GetComponent<Image>();
    }
    public void Refresh(int teamIndex)
    {
        TeamSkin.sprite = TeamImage[teamIndex];
    }
}
