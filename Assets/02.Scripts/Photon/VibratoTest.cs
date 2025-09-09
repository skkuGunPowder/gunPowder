using System;
using UnityEngine;
using DG.Tweening;

public class VibratoTest : MonoBehaviour
{
    public float Timer = 0;
    public float Vibrato = 1f;
    public float Interval = 1.5f;
    public float Power = 2f;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Play();   
        }
    }
    private void Play()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(Interval);
        sequence.Append(this.transform.DOShakePosition(Vibrato, Power));
    }
}

