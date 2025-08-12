using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test_PlayerInfo : MonoBehaviour
{
    public Camera Main;
    public Vector3 CameraPosition;
    public float duration = 1f;
    public Ease MyEase;
    
    // 씬 전환 테스트
    private void Awake()
    {
        Main = Camera.main;    
    }
    
    private void Start()
    {
        Play();
    }

    // Dotween 시작
    private void Play()
    {
        //카메라 이동
        Sequence sequence = DOTween.Sequence();
        sequence.Append(Main.transform.DOMove(CameraPosition, duration)).SetEase(MyEase);
        sequence.JoinCallback(SceneChange);
        
    }
    
    // 씬 전환
    private void SceneChange()
    {
        SceneManager.LoadScene("MapTest", LoadSceneMode.Additive);   
    }
}
