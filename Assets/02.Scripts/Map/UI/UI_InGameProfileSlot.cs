using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfileSlot : MonoBehaviour
{
    public TextMeshProUGUI NicknameTextUGUI;
    public TextMeshProUGUI GunpowderTextUGUI;
    public GameObject FirstPlace;
    public List<GameObject> LifeList;

    public void Init(string playerName)
    {
        NicknameTextUGUI.text = playerName;
    }
    public void Refresh(int gunpowder, int life)
    {
        GunpowderTextUGUI.text = gunpowder.ToString();
        LifeRefresh(life);
    }

    private void LifeRefresh(int life)
    {
        for (int i = 0; i < LifeList.Count; i++)
        {
            if(i < life)
            {
                LifeList[i].SetActive(true); 
            }
            else
            {
                LifeList[i].SetActive(false);
            }
        }
    }

    public void SetTop(bool isTop)
    {
        FirstPlace.SetActive(isTop);
    }
}
