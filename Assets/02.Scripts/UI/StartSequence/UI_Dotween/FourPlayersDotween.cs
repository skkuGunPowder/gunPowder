using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class FourPlayersDotween : MonoBehaviour
{

    public RectTransform Pivot;
    public LoadSceneChecker LoadChecker;
    
    public HorizontalLayoutGroup Layout;
    
    public RectTransform FirstPlayer;
    public RectTransform SecondPlayer;
    public RectTransform ThirdPlayer;
    public RectTransform FourthPlayer;
    
    public Image Black; 
    
    [Header("Slot")]
    public List<UI_ProductionSlot> UI_ProductionSlotList = new List<UI_ProductionSlot>();
    [Header("ForDotween")]
    public Vector2 StartPosition = new Vector2(250, 0);
    public Vector2 EndPosition = new Vector2(-30, 0);
    public float SpacingTime = 1f;
    public float IntervalTime = 1f;
    public float EndSpacing = 0;
    public float MiddleSpacing = 30;
    public float SecondMiddleSpacing = 50;
    public float StartSpacing = 20;
    public float VersusTime = 1f;
    public GameObject VSParticle;
    public RectTransform Versus;
    
    [Header("사운드")]
    public AudioClip VS_1;
    private void OnEnable()
    {
        EventManager.Instance.OnLoadEnd += OnLoadEnd;
        Play();
    }
    
    private void Play()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        
        sequence.Append(Pivot.DOAnchorPos(EndPosition, 1f).SetEase(Ease.OutCirc));
        sequence.Join(DOTween.To(() => Layout.spacing, x => Layout.spacing = x, MiddleSpacing, 1f)
            .SetEase(Ease.OutCirc));
        sequence.AppendCallback(SpacingDown);

    }

    private void SpacingDown()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(DOTween.To(() => Layout.spacing, x => Layout.spacing = x, EndSpacing, 0.4f).SetEase(Ease.InCirc));
        sequence.InsertCallback(VersusTime,VersusAct);
        sequence.AppendCallback(LightningOn);
        sequence.AppendCallback(ShakeOn);
        sequence.AppendCallback(FlashOn);
        sequence.AppendCallback(ShineOn);
        sequence.Insert(VersusTime,DOTween.To(() => Layout.spacing, x => Layout.spacing = x, -10f, 10f));
        sequence.InsertCallback(VersusTime * 3 ,LoadEnd);
    }

    
    // 모든 사람들의 로드가 끝난 후 적용되는 Dotween
    private void OnLoadEnd()
    {
        DOTween.KillAll();
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(FirstPlayer.DOAnchorPos(new Vector2(500, -1500f), 1f).SetEase(Ease.InBack));
        sequence.Join(FirstPlayer.DORotate(new Vector3(0,0,5f),0.5f).SetEase(Ease.InBack));
        sequence.Insert(0.2f , SecondPlayer.DOAnchorPos(new Vector2(960, -1500f), 1f).SetEase(Ease.InBack));   
        sequence.Insert(0.3f , SecondPlayer.DORotate(new Vector3(0,0,-5f),0.5f).SetEase(Ease.InBack));
        sequence.Insert(0.4f, ThirdPlayer.DOAnchorPos(new Vector2(1420, -1500),1f).SetEase(Ease.InBack));
        sequence.Insert(0.4f, ThirdPlayer.DORotate(new Vector3(0,0,5f),0.5f).SetEase(Ease.InBack));
        sequence.Insert(0.6f, FourthPlayer.DOAnchorPos(new Vector2(1880, -1500),1f).SetEase(Ease.InBack));
        sequence.Insert(0.6f, FourthPlayer.DORotate(new Vector3(0,0,-5f),0.5f).SetEase(Ease.InBack));
        sequence.Insert(0.8f, Versus.DOAnchorPos(new Vector2(0, -1500f), 1f).SetEase(Ease.InBack));
        sequence.Insert(0.8f, Black.DOFade(0, 1f));
        sequence.InsertCallback(2.4f, EventManager.Instance.LoadFinished);

        
    }
    
    // 나 로딩 완료
    private void LoadEnd()
    {
        Hashtable hash = new Hashtable()
        {
            {EProperties.IsLoad.ToString(), true}
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Layout.enabled = false;
    }

    
    private void ShineOn()
    {
        foreach (UI_ProductionSlot slot in UI_ProductionSlotList)
        {
            slot.ShineOn();
        }
    }

    private void FlashOn()
    {
        foreach (UI_ProductionSlot slot in UI_ProductionSlotList)
        {
            slot.FlashOn();
        }
    }
    
    private void ShakeOn()
    {
        foreach (UI_ProductionSlot slot in UI_ProductionSlotList)
        {
            slot.Shake();
        }
    }

    private void LightningOn()
    {
        VSParticle.gameObject.SetActive(true);
        SoundManager.Instance.PlayLocalSound(nameof(VS_1), transform, 0f, false, SoundType.SFX, true, 0.5f, 0.5f);
    }

    private void VersusAct()
    {
        Versus.gameObject.SetActive(true);
        Versus.DOScale(new Vector3(1, 1, 1), 0.15f).SetEase(Ease.OutCubic).SetUpdate(true);
    }
    
    private void OnDisable()
    {
        // 초기 위치로 리셋하기
        Pivot.anchoredPosition = StartPosition;
        Layout.spacing = StartSpacing;
        VSParticle.gameObject.SetActive(false);
        Versus.localScale = new Vector3(10, 10, 10);
        Versus.anchoredPosition = new Vector2(0, -273);
        Versus.gameObject.SetActive(false);
        FirstPlayer.rotation = Quaternion.Euler(0, 0, 0);
        SecondPlayer.rotation = Quaternion.Euler(0, 0, 0);
        ThirdPlayer.rotation = Quaternion.Euler(0, 0, 0);
        FourthPlayer.rotation = Quaternion.Euler(0, 0, 0);
        Layout.enabled = true;
        EventManager.Instance.OnLoadEnd -= OnLoadEnd;
        DOTween.Kill(this);
        
    }
}
