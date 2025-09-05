using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapFadeController : MonoBehaviour
{
    private float _fadeSpeed = 1.2f;
    public Tilemap Tilemap;
    public Color32 FadeColor;
    private void Awake()
    {
        EventManager.Instance.OnUltimate += FadeIn;
        EventManager.Instance.OnBackGroundFade += FadeOut;
    }
    
    private void FadeIn(string bomb, Photon.Realtime.Player player)
    {
        
    }
    
    private void FadeOut()
    {
        
    }
    
    private void OnDestroy()
    {
        
    }
}
