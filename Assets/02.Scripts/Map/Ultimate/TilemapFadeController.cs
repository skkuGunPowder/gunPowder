using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class TilemapFadeController : MonoBehaviour
{
    private float _fadeSpeed = 1.2f;
    
    public Tilemap Tilemap;
    public List<Tilemap> TilemapList;
    [Header("그룹이면 체크")]
    public bool IsGroup;

    private void Awake()
    {
        if (IsGroup)
        {
            TilemapList = new List<Tilemap>();
            
            EventManager.Instance.OnUltimate += FadeIn_Group;
            EventManager.Instance.OnBackGroundFade += FadeOut_Group;
            
            Tilemap[] renderers = this.GetComponentsInChildren<Tilemap>();
            
            foreach (Tilemap tilemap in renderers)
            {
                TilemapList.Add(tilemap);
            }
        }
        else
        {
            Tilemap = GetComponent<Tilemap>();
            EventManager.Instance.OnUltimate += FadeIn;
            EventManager.Instance.OnBackGroundFade += FadeOut;
        }
    }
    
    private void FadeIn(string bomb, Photon.Realtime.Player player)
    {
        DOTween.Kill(this);
        Tilemap.color = ColorPalette.ColorDictionary[EColorType.Fade];
    }
    
    private void FadeIn_Group(string bomb, Photon.Realtime.Player player)
    {
        DOTween.Kill(this);
        foreach (var tilemap in TilemapList)
        {
            tilemap.color = ColorPalette.ColorDictionary[EColorType.Fade];
        }
    }
    private void FadeOut()
    {
        DOTween.To(() => Tilemap.color, x => Tilemap.color = x, ColorPalette.ColorDictionary[EColorType.FadeOut], _fadeSpeed).SetUpdate(true);
    }

    private void FadeOut_Group()
    {
                
        foreach (Tilemap tile in TilemapList)
        {
            DOTween.To(() => tile.color, x => tile.color = x, ColorPalette.ColorDictionary[EColorType.FadeOut], _fadeSpeed).SetUpdate(true);
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
