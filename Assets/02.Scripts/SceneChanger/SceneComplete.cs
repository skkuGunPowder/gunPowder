using System;
using UnityEngine;
using MaskTransitions;
public class SceneComplete : MonoBehaviour
{
    [SerializeField] private float _duration = 1f;
    private void Start()
    {
        TransitionManager.Instance.EndAnimation(_duration);
    }
}
