using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class Cannon : MonoBehaviour
{
    [SerializeField] private GameObject _barrel;

    [SerializeField] private AudioClip _loadAudio;
    [SerializeField] private AudioClip _fireAudio;

    [SerializeField] private float _fireForce = 100f;
    [SerializeField] private float _fireDegree = 20f;

    private Rigidbody2D _firedProjectile;
    bool _isLoaded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isLoaded)
        {
            return;
        }

        _firedProjectile = collision.attachedRigidbody;

        StartCoroutine(FireRoutine(_firedProjectile));
    }

    private IEnumerator FireRoutine(Rigidbody2D target)
    {
        if (target.CompareTag("Player"))
        {
            InputHandler.BlockInput = true;
        }

        if (target.CompareTag("Bomb"))
        {
            target.GetComponent<Bomb>().ResetFuze();
        }

        _isLoaded = true;
        SoundManager.Instance.PlayLocalSound(_loadAudio.name, transform);

        target.linearVelocity = Vector2.zero;
        target.bodyType = RigidbodyType2D.Kinematic;
        target.transform.DOMove(_barrel.transform.position, 0.3f).SetEase(Ease.InQuad)
        .OnComplete(() =>
        {
            target.gameObject.SetActive(false);  
        });


        transform.DOScale(Vector3.one * 1.2f, 0.15f)
        .SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(0.3f);

        _barrel.transform.DORotate(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, _fireDegree), 0.5f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.3f);

        transform.DOScale(Vector3.one * 1.2f, 0.15f)
        .SetLoops(2, LoopType.Yoyo);

        target.gameObject.SetActive(true);
        target.bodyType = RigidbodyType2D.Dynamic;
        Vector2 fireDir = _barrel.transform.right.normalized;
        target.AddForce(fireDir * _fireForce, ForceMode2D.Impulse);

        SoundManager.Instance.PlayLocalSound(_fireAudio.name, transform);

        yield return null;

        if (target.gameObject.tag == "Player")
        {
            InputHandler.BlockInput = false;
        }

        _barrel.transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(0.5f);

        _isLoaded = false;
    }
}
