using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotationPlatform : MonoBehaviour
{
    public List<SidePlatform> SidePlatforms;
    private Vector3 _rotation;
    private Quaternion _initRotation;

    private void Awake()
    {
        _initRotation = transform.rotation;
        foreach (var sidePlatform in SidePlatforms)
        {
            sidePlatform.Init(this);
        }
    }

    public void Rotate(bool isClockWise)
    {
        if (isClockWise)
        {
            _rotation = new Vector3(0, 0, 180);
        }
        else
        {
            _rotation = new Vector3(0, 0, -180);
        }
        transform.DORotate(_rotation, 1.5f);
        // .OnComplete(() => transform.rotation = _initRotation);
    }
}
