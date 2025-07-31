using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotationPlatform : MonoBehaviour
{
    public List<SidePlatform> SidePlatforms;
    private Vector3 _rotation;
    private Quaternion _initRotation;
    private bool _isRotate = false;

    private void Awake()
    {
        // transform.eulerAngles = new Vector3(90, 90, 90);
        _initRotation = transform.rotation;
        foreach (var sidePlatform in SidePlatforms)
        {
            sidePlatform.Init(this);
        }
    }

    public void Rotate(bool isClockWise)
    {
        if (_isRotate)
        {
            return;
        }

        if (isClockWise)
        {
            _rotation = new Vector3(0, 0, -180);
        }
        else
        {
            _rotation = new Vector3(0, 0, 180);
        }
        
        _isRotate = true;

        transform.DORotate(transform.eulerAngles + _rotation, 1f)
        .OnComplete(() =>
        {
            _isRotate = false;   
        });
    }
}
