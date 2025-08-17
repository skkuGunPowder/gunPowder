using UnityEngine;
using System.Collections.Generic;

public class AirDropBox : MonoBehaviour
{
    public ParticleSystem LandingVFX;
    public List<AirDropItemBase> DropItemList;
    private bool _isInAir = true;


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("TileMap") && _isInAir)
        {
            _isInAir = false;
            ParticleSystem landingVFX = Instantiate(LandingVFX, other.contacts[0].point, Quaternion.identity);
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
