using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BabyCrab : Crab
{
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _druation = 5f;

    private Rigidbody2D _rigidbody;
    private float _changeDirectionTimer = 0f;
    private float _changeDirectionInterval;

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
        _changeDirectionInterval = Random.Range(1f, 4f);

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
        }

        while (timer < _druation)
        {
            timer += Time.deltaTime;
            transform.position = player.transform.position + new Vector3(0.5f, 0.5f, 0);

            _changeDirectionTimer += Time.deltaTime;
            if (_changeDirectionTimer >= _changeDirectionInterval)
            {
                _changeDirectionTimer = 0f;
                _changeDirectionInterval = Random.Range(1f, 4f);
                player.PlayerStat.FacingDirection *= -1;
                player.RPC_SetFacingDirection(player.PlayerStat.FacingDirection==-1 ? -1 : 1);
            }

            player.PlayerFSM.ChangeState<PlayerRunState>();
            yield return null;
        }

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
        }

        _rigidbody.simulated = true;
        _rigidbody.AddForce(new Vector2(1, 1).normalized * 1f, ForceMode2D.Impulse);
        StartCoroutine(DeactiveCoroutine());
    }
}
