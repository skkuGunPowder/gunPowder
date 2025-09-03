using System.Collections;
using UnityEngine;

public class BabyCrab : Crab
{
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _druation = 5f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Contact BabyCrab");
            if (_isActive)
            {
                return;
            }
            
            _isActive = true;
            if (_moveCoroutineInstance != null)
            {
                StopCoroutine(_moveCoroutineInstance);
            }

            Player player = collision.gameObject.GetComponent<Player>();
            StartCoroutine(BabyCrabCoroutine(player));
        }
    }

    private IEnumerator BabyCrabCoroutine(Player player)
    {
        float timer = 0f;
        _isNeedToMove = false;
        _rigidbody.simulated = false;

        // player.SetHasBabyCrab(true);
        while (timer < _druation)
        {
            timer += Time.deltaTime;
            transform.position = player.transform.position + new Vector3(0.5f, 0.5f, 0);
            yield return null;
        }
        // player.SetHasBabyCrab(false);

        _rigidbody.simulated = true;
        _rigidbody.AddForce(new Vector2(1, 1) * 1f, ForceMode2D.Impulse);
        StartCoroutine(DeactiveCoroutine());
    }
}
