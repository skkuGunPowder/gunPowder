using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinAnimationController : MonoBehaviour
{
    [SerializeField] private List<Animator> _animatorList;
    
    private void Awake()
    {
        _animatorList = new List<Animator>();
    }

    private void OnEnable()
    {
        Play("HitLoop");
    }

    public void Init()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        _animatorList.AddRange(animators);
    }

    public void Play(string animationName)
    {
        foreach (var animator in _animatorList)
        {
            animator.SetTrigger(animationName);
        }
    }
}
