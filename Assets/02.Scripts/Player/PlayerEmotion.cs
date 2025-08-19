using System;
using Photon.Pun;
using UnityEngine;

public class PlayerEmotion : MonoBehaviour
{
    private PhotonView _myPhotonView;
    public Animator MyAnimator;
    private void Awake()
    {
        _myPhotonView = GetComponent<PhotonView>();
    }

    public int GetPlayerNumber()
    {
        return _myPhotonView.Owner.ActorNumber;
    }

    public void Play(string emotionName)
    { 
        MyAnimator.SetTrigger(emotionName);
    }
    
    
}
