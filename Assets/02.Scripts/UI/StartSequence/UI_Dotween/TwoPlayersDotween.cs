using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;

public class TwoPlayersDotween : MonoBehaviour
{
    public RectTransform FirstPlayer;
    public RectTransform SecondPlayer;
    public RectTransform Versus;

    public List<UI_ProductionSlot> SlotList = new List<UI_ProductionSlot>();
    
    private void OnEnable()
    {
        Play();
    }

    private void Play()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(FirstPlayer.DOAnchorPos(new Vector2(530, -540), 0.7f).SetEase(Ease.OutCubic));
        sequence.Join(SecondPlayer.DOAnchorPos(new Vector2(1380, -540), 0.7f).SetEase(Ease.OutCubic));
        sequence.Insert(0.15f,Versus.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutCirc));
        sequence.Insert(0.7f,Versus.DOScale(new Vector3(1, 1, 1), 0.15f).SetEase(Ease.InCirc));
        sequence.Insert(0.75f, FirstPlayer.DOAnchorPos(new Vector2(600, -540), 0.1f).SetEase(Ease.InCirc));
        sequence.Insert(0.75f, SecondPlayer.DOAnchorPos(new Vector2(1310, -540), 0.1f).SetEase(Ease.InCirc));
        sequence.InsertCallback(0.8f, ShakeOn);
        sequence.InsertCallback(1f, ShineOn);
    }
    
    private void LoadEnd()
    {
        Hashtable hash = new Hashtable()
        {
            {EProperties.IsLoad.ToString(), true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    private void ObjectSetActive()
    {
        Versus.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        // 초기 위치로 리셋하기
        FirstPlayer.anchoredPosition = new Vector2(600, -540);
        SecondPlayer.anchoredPosition = new Vector2(1310, -540);
        Versus.anchoredPosition = new Vector2(960 , -540);
        Versus.localScale = new Vector3(0, 0, 0);
    }

    
    private void ShineOn()
    {
        foreach (UI_ProductionSlot slot in SlotList)
        {
            slot.ShineOn();
        }
    }
    private void ShakeOn()
    {
        foreach (UI_ProductionSlot slot in SlotList)
        {
            slot.Shake();
        }
    }
}
