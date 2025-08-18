using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class AirDropBox : MonoBehaviour
{
    public ParticleSystem LandingVFXPrefab;
    public ParticleSystem DropWarningVFXPrefab;
    public List<AirDropItemBase> DropItemList;
    public LayerMask layerMask;
    private bool _isInAir = true;

    private GameObject _dropWarningVFX;

    private void Awake()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 40f, layerMask);
        if (hit.collider == null)
        {
            return;
        }

        Debug.LogWarning($"Hitpoint [{hit.point}]");
        Debug.DrawLine(transform.position, hit.point, Color.red, 10f);

        _dropWarningVFX = PhotonNetwork.Instantiate(DropWarningVFXPrefab.name, hit.point, Quaternion.identity);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("TileMap") || other.gameObject.CompareTag("OneWayPlatform"))
        {
            if (!_isInAir)
            {
                return;
            }
            _isInAir = false;

            Destroy(_dropWarningVFX);

            ParticleSystem landingVFX = Instantiate(LandingVFXPrefab, other.contacts[0].point, Quaternion.identity);
            VFXPool.Instance.Get(LandingVFXPrefab.name);
            if (!_isInAir)
        {
            return;
        }
            landingVFX.Play();
            return;
        }

        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        Debug.LogWarning("박스 루팅");

        IAirDropItem item = DropItemList[Random.Range(0, DropItemList.Count)];
        item.SetOwner(other.gameObject.GetComponent<Player>());

        // TODO
        // 플레이어 장비에 장착

        Destroy(gameObject);
    }
}
