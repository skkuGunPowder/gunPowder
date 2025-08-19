using UnityEngine;

public class UI_EmotionSlot : MonoBehaviour
{
     private Animator _myAnimator;
     
     private void Awake()
     {
         _myAnimator = GetComponent<Animator>();
     }

     public void Play(string emotionName)
     {
         _myAnimator.SetTrigger(emotionName);
     }
}
