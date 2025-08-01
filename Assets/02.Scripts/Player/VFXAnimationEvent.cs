using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class VFXAnimationEvent : MonoBehaviour
{
    public Player Player;
    
    [Header("PlayerVFXPivot")]
    public Transform HeadPivot;
    public Transform LeftLegPivot;
    public Transform RightLegPivot;
    public Transform LeftPivot;
    public Transform RightPivot;

    [Header("PlayerVFX")]
    public ParticleSystem DashParticle;
    public ParticleSystem RunParticle;
    public ParticleSystem UpStrongAttackParticle;
    public ParticleSystem JumpParticle;
    public ParticleSystem JumpDashParticle;
    public ParticleSystem BreakParticle;
    public ParticleSystem LandParticle;


    // 다리 피벗에서 대시 파티클 재생
    public void DashVFXAtLeg()
    {
        LeftRightParticle(DashParticle, Quaternion.identity);
    }

    // 다리 피벗에서 대시 파티클 재생
    public void RunVFXAtLeg()
    {
        LeftRightParticle(RunParticle, Quaternion.identity);
    }

    private void LeftRightParticle(ParticleSystem particle, Quaternion rotation)
    {
        float playerFacingDirection = Player.PlayerStat.FacingDirection;
        Vector3 position = playerFacingDirection == 1 ? RightLegPivot.position : LeftLegPivot.position;
        ParticleSystem instantiatedParticle = Instantiate(particle, position, rotation);

        ParticleSystemRenderer renderer = instantiatedParticle.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            float noFlip = playerFacingDirection == 1 ? 0 : 1;
            Vector3 flip = renderer.flip;
            flip.x = noFlip;
            renderer.flip = flip;
        }
    }

    public void UpStrongAttackVFXAtHead()
    {
        Instantiate(UpStrongAttackParticle, HeadPivot.position, Quaternion.identity);
    }

    public void JumpVFXAtLeg()
    {
        LeftRightParticle(JumpParticle, Quaternion.identity);
    }

    public void JumpDashVFX()
    {
        float playerFacingDirection = Player.PlayerStat.FacingDirection;
        Vector3 position = playerFacingDirection == 1 ? RightLegPivot.position : LeftLegPivot.position;
        float x = playerFacingDirection == 1 ? 0 : -180;
        Quaternion rotation = Quaternion.Euler(x, -90, 0);
        Instantiate(JumpDashParticle, position, rotation);
    }

    public void BreakVFXAtLeg()
    {
        LeftRightParticle(BreakParticle, Quaternion.identity);
    }

    public void LandVFXLeg()
    {
        LeftRightParticle(LandParticle, Quaternion.identity);
    }
}
