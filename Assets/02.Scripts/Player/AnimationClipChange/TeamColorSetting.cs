using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TeamColorSetting : MonoBehaviour
{
    public TeamColor TeamColorList;
    private PhotonView _photonView;
    public Animator MyAnimator;

    private void Awake()
    {
        MyAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        EventManager.Instance.OnTeamChanged += Refresh;
    }

    private void Refresh()
    {
        
    }

    private void OnDisable()
    {
        EventManager.Instance.OnTeamChanged -= Refresh;
    }
}
