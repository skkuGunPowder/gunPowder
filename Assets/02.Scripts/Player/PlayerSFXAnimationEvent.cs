using UnityEngine;

public class PlayerSFXAnimationEvent : MonoBehaviour
{
    public const string MOVE_SFX_NAME_1 = "Footstep_1";
    public const string MOVE_SFX_NAME_2 = "Footstep_2";

    public const string GUNPOWDER_HEAL_SFX_NAME_1 = "GunPowderHeal_1";
    public const string GUNPOWDER_HEAL_SFX_NAME_2 = "GunPowderHeal_2";
    public const string GUNPOWDER_HEAL_SFX_NAME_3 = "GunPowderHeal_3";
    public const string GUNPOWDER_HEAL_SFX_NAME_4 = "GunPowderHeal_4";
    
    public const string PLAYER_ULTIMATE_USE_SFX_NAME = "PlayerUlt_1";

    public const string PLAYER_CRIT_DAMAGE_VOICE_SFX_NAME = "PlayerCritDamageVoice";

    public const string PLAYER_FALLDEAD_EXPLOSION_SFX_NAME = "FallDeadExplosion_1";
    public const string PLAYER_WITHOUT_ATTACK_SFX_NAME = "WithoutAttack_1";

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
    }

    public void PlayerUltimateUseSFX()
    {
        SoundManager.Instance.PlayLocalSound(PLAYER_ULTIMATE_USE_SFX_NAME, transform);
    }

    public void PlayerCritDamageVoiceRandomSFX()
    {
        SoundManager.Instance.PlayLocalRandomSound(PLAYER_CRIT_DAMAGE_VOICE_SFX_NAME, transform, 1, 2, 0f, false, SoundType.SFX, true, 1f, 50f);
    }

    public void PlayerFallDeadExplosionSFX()
    {
        SoundManager.Instance.PlayLocalSound(PLAYER_FALLDEAD_EXPLOSION_SFX_NAME, transform);
    }

    public void PlayerWithoutAttackSFX()
    {
        PlayerWithoutAttackSFX(1f);
    }

    public void PlayerWithoutAttackSFX(float pitch)
    {
        Sound sound = SoundManager.Instance.PlayLocalSound(PLAYER_WITHOUT_ATTACK_SFX_NAME, transform);
        if (sound == null) return;
        AudioSource audioSource = sound.GetAudioSource();
        if (audioSource != null)
        {
            audioSource.pitch = pitch;
        }
    }
}
