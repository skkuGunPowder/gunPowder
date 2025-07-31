using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPlatform : MonoBehaviour
{
    public List<SidePlatform> SidePlatforms;
    private float _rotation;
    private bool _isRotate = false;
    public bool IsRotate => _isRotate;

    private Rigidbody2D _rigdbody;

    private void Awake()
    {
        _rigdbody = GetComponent<Rigidbody2D>();
        _rigdbody.freezeRotation = true;
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
            _rotation = -180f;
        }
        else
        {
            _rotation = 180f;
        }

        StartCoroutine(RotateCoroutine(_rotation, 0.5f));
    }

    private IEnumerator RotateCoroutine(float angle, float duration)
    {
        _isRotate = true;
        _rigdbody.freezeRotation = false;

        float startRotation = _rigdbody.rotation;
        float targetRotation = startRotation + angle;
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            float newRotation = Mathf.Lerp(startRotation, targetRotation, t);
            _rigdbody.MoveRotation(newRotation);
            timer += Time.deltaTime;
            yield return null;
        }
        _rigdbody.MoveRotation(targetRotation);

        _rigdbody.freezeRotation = true;
        _isRotate = false;
    }
}
