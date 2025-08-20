using System;
using Photon.Pun;
using UnityEngine;

public class PlayerEmotion : MonoBehaviour
{
    private PhotonView _myPhotonView;
    public Animator MyAnimator;
    public bool IsLive => MyAnimator.gameObject.activeSelf; // 추후에 고쳐야함
    private void Awake()
    {
        _myPhotonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (_myPhotonView.IsMine)
        {
            EventManager.Instance.PlayerFind();   
        }
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
