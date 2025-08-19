using UnityEngine;

public class FireTruckAnimationEvent : MonoBehaviour
{
    public AudioClip StartAudio;
    public AudioClip DeployAudio;
    public AudioClip EndAudio;



    public void PlayStartSound()
    {
        SoundManager.Instance.PlayLocalSound(StartAudio.name, transform);
    }

    public void PlayDeploySound()
    {
        SoundManager.Instance.PlayLocalSound(DeployAudio.name, transform);
    }
    public void PlayEndSound()
    {
        SoundManager.Instance.PlayLocalSound(EndAudio.name, transform);
    }
}
