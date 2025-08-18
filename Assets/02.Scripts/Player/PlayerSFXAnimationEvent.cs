using UnityEngine;

public class PlayerSFXAnimationEvent : MonoBehaviour
{
    public const string MOVE_SFX_NAME_1 = "Footstep_1";
    public const string MOVE_SFX_NAME_2 = "Footstep_2";

    public const string GUNPOWDER_HEAL_SFX_NAME_1 = "GunPowderHeal_1";
    public const string GUNPOWDER_HEAL_SFX_NAME_2 = "GunPowderHeal_2";
    public const string GUNPOWDER_HEAL_SFX_NAME_3 = "GunPowderHeal_3";
    public const string GUNPOWDER_HEAL_SFX_NAME_4 = "GunPowderHeal_4";

    [SerializeField]
    private float _gunpowderHealWindow = 0.25f;
    private float _lastGunpowderHealTime = -999f;
    private int _gunpowderHealLevel = 0; // 0 idle, 1..4 active

    public void MoveSFX1()
    {
        SoundManager.Instance.PlayLocalSound(MOVE_SFX_NAME_1, transform);
    }

    public void MoveSFX2()
    {
        SoundManager.Instance.PlayLocalSound(MOVE_SFX_NAME_2, transform);
    }

    // 플레이어가 건파우더를 흡수했을 때 호출
    public void OnGunpowderAbsorbed()
    {
        float now = Time.time;
        if (now - _lastGunpowderHealTime <= _gunpowderHealWindow)
        {
            _gunpowderHealLevel = Mathf.Min(_gunpowderHealLevel + 1, 4);
        }
        else
        {
            _gunpowderHealLevel = 1;
        }
        _lastGunpowderHealTime = now;

        // 사운드 재생 (요청에 따라 주석 처리; 원하면 아래 주석 해제)
        string sfx = _gunpowderHealLevel switch
        {
            1 => GUNPOWDER_HEAL_SFX_NAME_1,
            2 => GUNPOWDER_HEAL_SFX_NAME_2,
            3 => GUNPOWDER_HEAL_SFX_NAME_3,
            _ => GUNPOWDER_HEAL_SFX_NAME_4,
        };
        SoundManager.Instance.PlayLocalSound(sfx, transform);
        Debug.Log(sfx);
    }
}
