using UnityEngine;

public class GunPowder : MonoBehaviour
{
    private BoxCollider2D _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 0.2f;
    private Transform _target;
    public Transform Target => _target;

    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.enabled = false;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > _colliderOnTime)
        {
            _collider.enabled = true;
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
