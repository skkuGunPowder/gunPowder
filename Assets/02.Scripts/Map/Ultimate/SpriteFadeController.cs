using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpriteFadeController : MonoBehaviour
{
    public SpriteRenderer MapSpriteRenderer;
    public List<SpriteRenderer> MapSpriteRendererList;
    private float _fadeSpeed = 1.2f;
    [Header("스프라이트 그룹이면 체크")]
    public bool IsGroup;

    private void Awake()
    {
        if (IsGroup)
        {
            MapSpriteRendererList = new List<SpriteRenderer>();
            
            EventManager.Instance.OnUltimate += FadeIn_Group;
            EventManager.Instance.OnBackGroundFade += FadeOut_Group;
            
            SpriteRenderer[] renderers = this.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sprite in renderers)
            {
                MapSpriteRendererList.Add(sprite);
            }
        }
        else
        {
            MapSpriteRenderer = GetComponent<SpriteRenderer>();
            EventManager.Instance.OnUltimate += FadeIn;
            EventManager.Instance.OnBackGroundFade += FadeOut;
        }
    }
    
    private void FadeIn(string bomb, Photon.Realtime.Player player)
    {
        DOTween.Kill(this);
        MapSpriteRenderer.color = ColorPalette.ColorDictionary[EColorType.Fade];
    }
    
    private void FadeIn_Group(string bomb, Photon.Realtime.Player player)
    {
        DOTween.Kill(this);
        foreach (SpriteRenderer tilemap in MapSpriteRendererList)
        {
            tilemap.color = ColorPalette.ColorDictionary[EColorType.Fade];
        }
    }
    private void FadeOut()
    {
        DOTween.To(() => MapSpriteRenderer.color, x => MapSpriteRenderer.color = x, ColorPalette.ColorDictionary[EColorType.FadeOut], _fadeSpeed);
    }

    private void FadeOut_Group()
    {
                
        foreach (SpriteRenderer tile in MapSpriteRendererList)
        {
            DOTween.To(() => tile.color, x => tile.color = x, ColorPalette.ColorDictionary[EColorType.FadeOut], _fadeSpeed);
        }
    }
    
    private void OnDestroy()
    {
        if (IsGroup)
        {
            EventManager.Instance.OnUltimate -= FadeIn_Group;
            EventManager.Instance.OnBackGroundFade -= FadeOut_Group;
        }
        else
        {
            EventManager.Instance.OnUltimate -= FadeIn;
            EventManager.Instance.OnBackGroundFade -= FadeOut;
        }
    }
}
