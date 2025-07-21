using UnityEngine;

public class GunPowder : MonoBehaviour
{
    private Collider _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 2f;

    private void Start()
    {
        _collider = GetComponent<Collider>();
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

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
