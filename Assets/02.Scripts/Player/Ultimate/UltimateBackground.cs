using System;
using UnityEngine;

public class UltimateBackground : MonoBehaviour
{
    public RectTransform Wind1;
    public RectTransform Wind2;
    
    public float WindSpeed = 1f;
    public Vector2 Wind1OriginPosition;
    public Vector2 Wind2OriginPosition;
    public Vector2 WindResetPosition;
    public Vector2 WindInitPosition;
    private void OnEnable()
    {
        Wind1.anchoredPosition = Wind1OriginPosition;
        Wind2.anchoredPosition = Wind2OriginPosition;
    }

    private void Update()
    {
        MoveWind(Wind1);
        MoveWind(Wind2);
    }

    // private void Play()
    // {
    //     float wind1 = WindSpeed * Time.deltaTime;
    //     Wind1.anchoredPosition = Vector2.Lerp(Wind1.anchoredPosition, WindResetPosition, -WindSpeed * Time.deltaTime);
    //     Wind2.anchoredPosition = Vector2.Lerp(Wind2.anchoredPosition, WindInitPosition, -WindSpeed * Time.deltaTime);
    //     
    //     Debug.Log(Wind1.anchoredPosition);
    //
    //     if (Wind1.anchoredPosition == WindResetPosition)
    //     {
    //         Wind1.anchoredPosition = WindInitPosition;
    //     }
    //
    //     if (Wind2.anchoredPosition == WindInitPosition)
    //     {
    //         Wind2.anchoredPosition = WindInitPosition;
    //     }
    // }
    
    private void MoveWind(RectTransform wind)
    {
        // 왼쪽으로 이동
        wind.anchoredPosition += Vector2.left * WindSpeed * Time.deltaTime;

        // 특정 위치 지나면 다시 오른쪽으로 보냄
        if (wind.anchoredPosition.x <= WindResetPosition.x)
        {
            wind.anchoredPosition = WindInitPosition;
        }
    }
}
