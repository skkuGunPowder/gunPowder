using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Observing : MonoBehaviour
{
    private CameraController _cameraController;
    public GameObject ObservingPanel;
    
    public TextMeshProUGUI NicknameText;
    public Image ObservingImage;
    public Button UpButton;
    public Button BackButton;
    [Header("Dotween")]
    public float FadeDuration = 1.5f;
    
    private void Awake()
    {
        if (Camera.main != null)
        {
            _cameraController = Camera.main.GetComponent<CameraController>();   
        }
        else
        {
            _cameraController = FindFirstObjectByType<CameraController>();  
        }

        if (_cameraController == null)
        {
            throw new Exception("현재 씬에 CameraController 스크립트가 없습니다.");
        }
        
        _cameraController.OnNicknameChanged += Refresh;
        _cameraController.OnUIOnOff += OnOff;
    }
    
    private void Refresh(string nickname)
    {
        NicknameText.text = nickname;
    }

    private void OnOff(bool isOn)
    {
        Debug.Log("onoff");
        if (isOn)
        {
            ObservingPanel.SetActive(true);
            ObjectActive(1);
        }
        else
        {
            ObjectActive();
        }
    }

    private void ObjectActive(float value)
    {
        ObservingImage.DOFade(value, FadeDuration);
        NicknameText.DOFade(value, FadeDuration);
        UpButton.image.DOFade(value, FadeDuration);
        BackButton.image.DOFade(value, FadeDuration);
    }

    private void ObjectActive()
    {
        NicknameText.DOFade(0, FadeDuration);
        UpButton.image.DOFade(0, FadeDuration);
        BackButton.image.DOFade(0, FadeDuration);
        ObservingImage.DOFade(0, FadeDuration).OnComplete(() =>
        {
            DOTween.Kill(this);
            ObservingPanel.SetActive(false);
        });
    }
    
    public void OnClickSetTarget(int index)
    {
        _cameraController.SelectTarget(index);
    }

    private void OnDisable()
    {
        _cameraController.OnNicknameChanged -= Refresh;
        _cameraController.OnUIOnOff -= OnOff;
    }
}
