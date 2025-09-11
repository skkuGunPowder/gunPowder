using System.Collections;
using Photon.Pun;
using UnityEngine;

public class KingCrab : Crab
{
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private float _holdDuration = 5f;
    [SerializeField] private float _throwForce = 10f;

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
            Player player = collision.gameObject.GetComponent<Player>();
            StartCoroutine(HoldCoroutine(player));
        }
    }

    private IEnumerator HoldCoroutine(Player player)
    {
        float randomThrowX = Random.Range(0, 2) == 0 ? -1 : 1;
        Vector2 throwDirection = new Vector2(randomThrowX, 1).normalized;

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
            player.RPC_SetAnimatorTrigger("HitLoop");
        }

        float timer = 0f;
        while (timer < _holdDuration)
        {
            timer += Time.deltaTime;
            player.transform.position = _holdPoint.position;
            player.transform.rotation = _holdPoint.rotation;
            yield return null;
        }
        player.transform.rotation = Quaternion.identity;
        player.Rigidbody2D.AddForce(throwDirection * _throwForce, ForceMode2D.Impulse);
        player.PhotonView.RPC(nameof(player.RPC_ChangeState), RpcTarget.All, nameof(PlayerDamagedState));

        if (player.PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
        }

        StartCoroutine(DeactiveCoroutine());
    }
}
