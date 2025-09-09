using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;
using PhotonPlayer = Photon.Realtime.Player;
public class UltimateEffectLocal : MonoBehaviour
{
    public List<GameObject> EffectList;
    private SpriteRenderer _ultiBackGround;
    
    [Header("궁극기에 얼마나 멈출건지")]
    public float UltiTime = 1f;
    public float ColorChangeTime = 0.1f;
    public float FadeTime = 1.2f;
    public GameObject UltimateBackGround;
    
    private List<PhotonView> _playerList = new List<PhotonView>();
    private void Awake()
    {
        EventManager.Instance.OnUltimate += PlayEffect;
        EventManager.Instance.OnPlayerListUp += PlayerListUp;
        _ultiBackGround = UltimateBackGround.GetComponent<SpriteRenderer>();
    }

    private void PlayEffect(string bomb, PhotonPlayer player)
    {
        DOTween.Kill(this);
        StopAllCoroutines();
        foreach (PhotonView view in _playerList)
        {
            if (view.OwnerActorNr == player.ActorNumber)
            {
                EffectPool().transform.position = view.transform.position;
                break;
            }
        }

        UltimateOn();
        StartCoroutine(BackGroundColorChange());

    }

    private GameObject EffectPool()
    {
        foreach (GameObject effect in EffectList)
        {
            if (effect.activeSelf == false)
            {
                effect.SetActive(true);   
                return effect;
            }
        }
        
        return null;
    }
    private void UltimateOn()
    {
        StartCoroutine(TimeSlow());
    }
    
    private IEnumerator TimeSlow()
    {
        float time = 0;
        GameManager.Instance.GameStateChange(EGameState.Ultimate);
        
        while (time < UltiTime)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        
        GameManager.Instance.GameStateChange(EGameState.Playing);
        BackGroundOff();
        EventManager.Instance.BackGroundFade();
        
    }   
    private void PlayerListUp()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _playerList.Clear();
        
        foreach (var player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            _playerList.Add(view);
        }
        
        Debug.Log($"플레이어 리스트 업 : {_playerList.Count}");
        EventManager.Instance.OnPlayerListUp -= PlayerListUp;
    }

    private IEnumerator BackGroundColorChange()
    {
        float time = 0;
        UltimateBackGround.SetActive(true);
        while (time < ColorChangeTime)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        
        _ultiBackGround.color = ColorPalette.ColorDictionary[EColorType.UltiBack];
    }
    private void BackGroundOff()
    {
        _ultiBackGround.DOFade(0, FadeTime).OnComplete(() =>
        {
            UltimateBackGround.SetActive(false);
            _ultiBackGround.color = Color.white; 
        });
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnUltimate -= PlayEffect;
    }
}
