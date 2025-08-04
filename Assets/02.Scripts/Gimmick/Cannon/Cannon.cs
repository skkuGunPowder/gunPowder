using UnityEngine;
using DG.Tweening;

public class Cannon : MonoBehaviour
{
    [SerializeField] private GameObject _barrel;

    [SerializeField] private float _fireForece = 10f;
    [SerializeField] private float _fireDegree = 20f;

    private Rigidbody2D _firedProjectile;
    bool _isLoaded = false;


    private void Fire()
    {
        _firedProjectile.AddForce(_barrel.transform.right * _fireForece, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isLoaded)
        {
            return;
        }

        _firedProjectile = collision.gameObject.GetComponent<Rigidbody2D>();
        _isLoaded = true;

        _barrel.transform.DORotate(new Vector3(0, 0, _fireDegree), 0.5f)
        .OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.3f, () => Fire());
            _barrel.transform.DORotate(new Vector3(0, 0, 0), 0.5f)
            .OnComplete(() =>
            {
                _isLoaded = false;
                _firedProjectile = null;
            });
        });
    }
}
