using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ResultProduction : MonoBehaviour
{
    private Camera _camera;
    public RectTransform CameraObject;
    public Image BlackOut;
    private float _timer;
    public float FireWorksTime;
    public float FireWorksSpacingTime;
    public int FireWorksPercent;
    public List<GameObject> FireWorksParticle;
    
    [Header("For Dotween")]
    [Header("암전 관련")]
    public float FadeSpeed;
    public float FadeAmount;
    public Ease FadeEaseType;
    [Header("메인 카메라 관련")]
    public float OthorSize;
    public float MoveSpeed;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Ease CameraEaseType;
    
    [Header("카메라 이미지 관련")]
    public float ImageScaleSpeed = 0.2f;
    public Vector3 ImageStartScale;
    public Ease ImageEaseType;
    public float ImageIntervalTime;

    [Header("모든 오브젝트 초기화 값")] 
    public Vector3 CameraImageScale;
    public float CameraOthorSize;
    public Color32 FadeColor;
    
    // 시작하자마자 연출 시작
    private void Awake()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            throw new Exception("카메라가 없음");
        }
        _camera.orthographicSize = CameraOthorSize;
    }

    private void OnEnable()
    {
        Play();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        FireWorks();
    }
    private void Play()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(BlackOut.DOFade(FadeAmount,FadeSpeed).SetEase(FadeEaseType));
        sequence.Append(_camera.DOOrthoSize(OthorSize,MoveSpeed)).SetEase(CameraEaseType);
        sequence.Join(CameraObject.DOScale(ImageStartScale, ImageScaleSpeed)).SetEase(ImageEaseType);
        sequence.AppendInterval(ImageIntervalTime);
        sequence.AppendCallback(EventManager.Instance.ViewGameResult);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        CameraObject.localScale = CameraImageScale;
        BlackOut.color = FadeColor;
    }

    private void FireWorks()
    {
        if (_timer < FireWorksTime)
        {
            return;
        }

        _timer = 0;
        
        Task_FireWorks().Forget();
    }

    private async UniTaskVoid Task_FireWorks()
    {
        foreach (GameObject particle in FireWorksParticle)
        {
            int random = UnityEngine.Random.Range(0, FireWorksPercent);

            if (random != 0)
            {
                continue;
            }
            
            particle.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(FireWorksSpacingTime));
        }
    }
}
