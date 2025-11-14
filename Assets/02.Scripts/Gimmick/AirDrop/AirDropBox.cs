using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Collections;

public class AirDropBox : MonoBehaviour
{
    public ParticleSystem LandingVFXPrefab;
    public ParticleSystem DropWarningVFXPrefab;
    public List<AirDropItemBase> DropItemList;
    public LayerMask layerMask;
    private bool _isInAir = true;

    private ParticleSystem _dropWarningVFX;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 40f, layerMask);
        if (hit.collider == null)
        {
            return;
        }

        _dropWarningVFX = Instantiate(DropWarningVFXPrefab, hit.point, Quaternion.identity);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_dropWarningVFX != null)
        {
            _dropWarningVFX.gameObject.SetActive(false);
            Destroy(_dropWarningVFX);
        }

        if (other.gameObject.layer == 6)
        {
            if (!_isInAir)
            {
                return;
            }

            _isInAir = false;
            _animator.SetBool("IsInAir", _isInAir);

            VFX landingVFX = VFXPool.Instance.Get(LandingVFXPrefab.name);
            landingVFX.transform.position = transform.position;
            landingVFX.Play();
            return;
        }

        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
        {
            return;
        }
        
        Player player = other.gameObject.GetComponent<Player>();

        if (player.AirDropItem == null)
        {
            AirDropItemBase item = DropItemList[Random.Range(0, DropItemList.Count)];
            item.SetOwner(player);
            player.SetAirDropItem(item);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DestoryCoroutine(gameObject));
        }
    }

    private IEnumerator DestoryCoroutine(GameObject toDestroyObject)
    {
        yield return null;
        PhotonNetwork.Destroy(toDestroyObject);
    }
}
