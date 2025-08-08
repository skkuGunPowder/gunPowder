using System;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class GameOverProduction : MonoBehaviour
{
    private Camera _camera;
    [Header("위치 액션")] public RectTransform GameOverProductionPanel;
    public Vector2 FirstPotion;
    public Vector2 SecondPotion;
    public Vector2 ThirdPotion;
    public Vector2 FourthPotion;
    public Vector2 FifthPotion;
    public Vector2 SixthPotion;

    [Header("카메라 액션")] public GameObject CameraObject;
    public GameObject BlackOut;

    private void OnEnable()
    {
        _camera = Camera.main;
    }

    public void Play()
    {
        GameOverProductionPanel.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(GameOverProductionPanel.DOAnchorPos(FirstPotion, 0.5f).SetEase(Ease.OutBack));
        sequence.Append(GameOverProductionPanel.DOAnchorPos(SecondPotion, 0.3f).SetEase(Ease.OutBack));
        sequence.Append(GameOverProductionPanel.DOAnchorPos(ThirdPotion, 0.4f).SetEase(Ease.OutBack));
        sequence.Append(GameOverProductionPanel.DOAnchorPos(FourthPotion, 0.3f).SetEase(Ease.OutBack));
        sequence.Append(GameOverProductionPanel.DOAnchorPos(FifthPotion, 0.2f).SetEase(Ease.OutBack));
        sequence.Append(GameOverProductionPanel.DOAnchorPos(SixthPotion, 0.3f).SetEase(Ease.OutBack));
        sequence.AppendCallback(GameManager.Instance.GameResultCheck);
        sequence.AppendInterval(1f);
        sequence.Append(_camera.DOOrthoSize(15f, 1f).SetEase(Ease.InCirc));
        sequence.AppendCallback(CameraOn);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(CameraOff);
        sequence.AppendCallback(GameManager.Instance.GameResultCheck);
        sequence.AppendInterval(0.5f);
        sequence.OnComplete(() =>
        {
            GameManager.Instance.GameStateChange(EGameState.GameOver);

            if (PhotonNetwork.IsMasterClient == false)
            {
                return;
            }
            
            if (!PhotonNetwork.IsMessageQueueRunning)
                return; // 또는 로딩 상태 체크

            PhotonNetwork.IsMessageQueueRunning = false;

            PhotonNetwork.LoadLevel(ESceneList.Map4.ToString());
        });

    }

    private void CameraOn()
    {
        CameraObject.SetActive(true);
    }
    
    private void CameraOff()
    {
        BlackOut.SetActive(true);
    }
    private void OnDisable()
    {
        DOTween.KillAll();
    }
}