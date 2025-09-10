using System;
using UnityEngine;
using DG.Tweening;

public class VibratoTest : MonoBehaviour
{
    public float Timer = 0;
    public float VibrateDuration = 1f;
    public float VibratePower = 1f;
    public int Vibrato = 20;
    public float Interval = 1.5f;
    public float Randomness = 0.5f;
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
        sequence.Append(this.gameObject.transform.DOShakePosition(VibrateDuration, VibratePower, Vibrato,Randomness, false,true, ShakeRandomnessMode.Harmonic));

    }
}

