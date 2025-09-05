using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
using DG.Tweening;
public class HitScreen : MonoBehaviour
{
    public Image HitScreenImage;
    public int Value = 80;
    public int MaxValue = 230;
    public float FadeSpeed = 0.5f;
    public float FadeInSpeed = 0.3f;
    public Color ScreenColor = Color.white;
    private void Awake()
    {
        EventManager.Instance.OnHitScreen += PlayHitScreen;
        ScreenColor = HitScreenImage.color;
    }

    private void PlayHitScreen()
    {
        DOTween.Kill(this);

        int currentValue = (int)HitScreenImage.color.a;
        currentValue = Mathf.Min(currentValue + Value, MaxValue);
        Color newColor = new Color(ScreenColor.r, ScreenColor.g, ScreenColor.b, currentValue);
        FadeIn(newColor);
    }

    private void FadeIn(Color newColor)
    {
        HitScreenImage.DOColor(newColor, FadeInSpeed).OnComplete(() =>
        {
            FadeOut();   
        });
    }

    
    private void FadeOut()
    {
        HitScreenImage.DOColor(ScreenColor, FadeSpeed);
    }
    private void OnDestroy()
    {
        EventManager.Instance.OnHitScreen -= PlayHitScreen;
    }

}
