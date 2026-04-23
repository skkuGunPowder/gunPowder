using System;
using DG.Tweening;

[Serializable]
public class ShakePower
{
    private const float DEFAULT_DURATION = 1f;
    private const float DEFAULT_POWER = 0.65f;
    private const int DEFAULT_VIBRATO = 110;
    
    public float VibrateDuration = 1f;
    public float VibratePower = 0.65f;
    public int Vibrato = 110;
    public Ease ShakeEase = Ease.Linear;
    public ShakePower()
    {
    }
    
    public ShakePower(float vibrateDuration, float vibratePower, int vibrato) 
    {
        VibrateDuration = vibrateDuration;
        VibratePower = vibratePower;
        Vibrato = vibrato;
    }

    public void SetDefaults()
    {
        VibrateDuration = DEFAULT_DURATION;
        VibratePower = DEFAULT_POWER;
        Vibrato = DEFAULT_VIBRATO;
    }
}