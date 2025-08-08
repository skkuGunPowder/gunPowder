using System.Collections.Generic;
using UnityEngine;

public class Jet : MonoBehaviour
{
    [SerializeField] protected Transform _spawnTransform;
    [SerializeField] protected List<GameObject> _dropItemList;
    [SerializeField] protected float _speed;
    [SerializeField] protected float _dropCoolTime;
    [SerializeField] protected int _dropAmount;

    private float _timer;

    private void OnEnable()
    {
        _timer = 0f;

        _spawnTransform = GameObject.FindWithTag("JetSpawn").transform;
        if (_spawnTransform == null)
        {
            Debug.LogError("비행기 스폰 포인트가 없습니다.");
            return;
        }
        transform.position = _spawnTransform.position;
    }

    protected virtual void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _dropCoolTime)
        {
            _timer = 0f;
            DropItem();
        }

        transform.position += transform.right * _speed * Time.deltaTime;
    }

    protected virtual void DropItem()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadLine"))
        {
            Destroy(gameObject);
        }
    }
}
