using System;
using UnityEngine;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;

public class TwoPlayersDotween : MonoBehaviour
{
    public RectTransform FirstPlayer;
    public RectTransform SecondPlayer;
    
    private void OnEnable()
    {
        FirstPlayer.DOAnchorPos(new Vector2(700,-540), 5);
        SecondPlayer.DOAnchorPos(new Vector2(1220,540), 5).OnComplete(() =>
        {
            Hashtable hash = new Hashtable()
            {
                {EProperties.IsLoad.ToString(), true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        });    
    }
    
    private void OnDisable()
    {
        FirstPlayer.DOAnchorPos(new Vector2(0,0), 1);
        SecondPlayer.DOAnchorPos(new Vector2(0,0), 1);    
    }
}
