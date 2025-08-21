using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;

public class TwoPlayersDotween : MonoBehaviour
{
    public LoadSceneChecker LoadChecker;
    [Header("DOTween용")]
    public RectTransform FirstPlayer;
    public RectTransform SecondPlayer;
    public RectTransform Versus;
    public List<UI_ProductionSlot> SlotList = new List<UI_ProductionSlot>();
    public Image Black;
    
    [Header("파티클")] 
    public GameObject VSParicle;
    
    
    private void OnEnable()
    {
        EventManager.Instance.OnLoadEnd += OnLoadEnd;
        Play();
    }

    private void Play()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(FirstPlayer.DOAnchorPos(new Vector2(530, -540), 0.7f).SetEase(Ease.OutCubic));
        sequence.Join(SecondPlayer.DOAnchorPos(new Vector2(1380, -540), 0.7f).SetEase(Ease.OutCubic));
        sequence.Insert(0.15f,Versus.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutCirc));
        sequence.Insert(0.7f,Versus.DOScale(new Vector3(1, 1, 1), 0.15f).SetEase(Ease.InCirc));
        sequence.Insert(0.75f, FirstPlayer.DOAnchorPos(new Vector2(600, -540), 0.1f).SetEase(Ease.InCirc));
        sequence.Insert(0.75f, SecondPlayer.DOAnchorPos(new Vector2(1310, -540), 0.1f).SetEase(Ease.InCirc));
        sequence.InsertCallback(0.9f, ShakeOn);
        sequence.InsertCallback(0.9f, LightningOn);
        sequence.InsertCallback(0.9f, FlashOn);
        sequence.InsertCallback(1f, ShineOn);
        sequence.Insert(1.2f,FirstPlayer.DOAnchorPos(new Vector2(610, -540), 10f));
        sequence.Insert(1.2f, SecondPlayer.DOAnchorPos(new Vector2(1300, -540), 10f));
        sequence.InsertCallback(2f,LoadEnd);

    }

    private void OnLoadEnd()
    {
        DOTween.KillAll();
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(FirstPlayer.DOAnchorPos(new Vector2(610, -1500f), 1f).SetEase(Ease.InBack).SetUpdate(true));
        sequence.Join(FirstPlayer.DORotate(new Vector3(0,0,3f),0.5f).SetEase(Ease.InBack).SetUpdate(true));
        sequence.Insert(0.2f , SecondPlayer.DOAnchorPos(new Vector2(1300, -1500f), 1f).SetEase(Ease.InBack).SetUpdate(true));   
        sequence.Insert(0.3f , SecondPlayer.DORotate(new Vector3(0,0,-3f),0.5f).SetEase(Ease.InBack).SetUpdate(true));
        sequence.Insert(0.3f, Versus.DOAnchorPos(new Vector2(0, -1500f), 1f).SetEase(Ease.InBack).SetUpdate(true));
        sequence.Insert(0.3f, Black.DOFade(0, 1f).SetUpdate(true));
        sequence.InsertCallback(2f, EventManager.Instance.LoadFinished).SetUpdate(true);

    }
    private void LoadEnd()
    {
        Hashtable hash = new Hashtable()
        {
            {EProperties.IsLoad.ToString(), true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnLoadEnd -= OnLoadEnd;
        
        // 초기 위치로 리셋하기
        FirstPlayer.anchoredPosition = new Vector2(600, -540);
        FirstPlayer.rotation = Quaternion.Euler(0, 0, 0);
        SecondPlayer.anchoredPosition = new Vector2(1310, -540);
        SecondPlayer.rotation = Quaternion.Euler(0, 0, 0);
        Versus.anchoredPosition = new Vector2(0 , -0);
        Versus.localScale = new Vector3(0, 0, 0);
        VSParicle.gameObject.SetActive(false);
        DOTween.Kill(this);
    }

    
    private void ShineOn()
    {
        foreach (UI_ProductionSlot slot in SlotList)
        {
            slot.ShineOn();
        }
    }

    private void FlashOn()
    {
        foreach (UI_ProductionSlot slot in SlotList)
        {
            slot.FlashOn();
        }
    }
    private void ShakeOn()
    {
        foreach (UI_ProductionSlot slot in SlotList)
        {
            slot.Shake();
        }
    }

    private void LightningOn()
    {
        VSParicle.gameObject.SetActive(true);
    }
}
