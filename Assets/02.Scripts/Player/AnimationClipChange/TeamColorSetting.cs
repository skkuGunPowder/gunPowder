using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TeamColorSetting : MonoBehaviour
{
    public List<TeamColor> TeamColorList;
    private PhotonView _photonView;
    public Animator MyAnimator;
    private void Awake()
    {
        MyAnimator = GetComponent<Animator>();
        _photonView = GetComponentInParent<PhotonView>();
    }

    private void OnEnable()
    {
        EventManager.Instance.OnTeamChanged += Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_photonView.IsMine == false)
        {
            return;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false)
        {
            MyAnimator.runtimeAnimatorController = TeamColorList[0].TeamAnimator;
            return;       
        }
        
        EInGameTeam myteam = (EInGameTeam)PhotonNetwork.LocalPlayer.CustomProperties[EProperties.Team.ToString()];
        
        foreach (TeamColor color in TeamColorList)
        {
            if (color.Team == myteam)
            {
                MyAnimator.runtimeAnimatorController = color.TeamAnimator;
                break;
            }
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnTeamChanged -= Refresh;
    }
}
