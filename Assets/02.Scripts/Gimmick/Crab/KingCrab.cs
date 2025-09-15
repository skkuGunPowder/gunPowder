using System.Collections;
using Photon.Pun;
using UnityEngine;

public class KingCrab : Crab
{
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private float _holdDuration = 5f;
    [SerializeField] private float _throwForce = 10f;

    private Rigidbody2D _rigidbody;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Contact KingCrab");
            if (_isActive)
            {
                return;
            }

            _animator.SetBool("IsMoving", false);
            _animator.SetBool("IsAttack", true);

            _isActive = true;
            
            if (_moveCoroutineInstance != null)
            {
                StopCoroutine(_moveCoroutineInstance);
            }

            Player player = collision.gameObject.GetComponent<Player>();
            StartCoroutine(HoldCoroutine(player));
        }
    }

    private IEnumerator HoldCoroutine(Player player)
    {
        _isNeedToMove = false;

        float randomThrowX = Random.Range(0, 2) == 0 ? -1 : 1;
        Vector2 throwDirection = new Vector2(randomThrowX, 1).normalized;

        _rigidbody.linearVelocity = Vector2.zero;

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
            //player.RPC_SetAnimatorTrigger("HitLoop");
            player.PhotonView.RPC(nameof(player.RPC_ChangeState), RpcTarget.All, nameof(PlayerCrabHoldedState));
        }

        float timer = 0f;
        while (timer < _holdDuration)
        {
            timer += Time.deltaTime;
            player.transform.position = _holdPoint.position;
            player.transform.rotation = _holdPoint.rotation;
            yield return null;
        }
        player.PhotonView.RPC(nameof(player.RPC_ChangeState), RpcTarget.All, nameof(PlayerDamagedState));
        
        player.transform.rotation = Quaternion.identity;
        player.Rigidbody2D.AddForce(throwDirection * _throwForce, ForceMode2D.Impulse);

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
        }

        StartCoroutine(DeactiveCoroutine());
    }
}
