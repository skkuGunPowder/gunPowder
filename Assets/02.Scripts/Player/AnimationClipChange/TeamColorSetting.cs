using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TeamColorSetting : MonoBehaviour
{
    public List<TeamColor> TeamColorList;
    public Animator MyAnimator;
    
    private void Awake()
    {
        MyAnimator = GetComponent<Animator>();
    }

    public void Refresh(EInGameTeam team)
    {
        foreach (TeamColor color in TeamColorList)
        {
            if (color.Team == team)
            {
                MyAnimator.runtimeAnimatorController = color.TeamAnimator;;   
                break;
            }
        }
    }
}
