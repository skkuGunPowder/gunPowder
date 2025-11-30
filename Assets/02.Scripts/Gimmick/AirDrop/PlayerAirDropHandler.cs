using UnityEngine;
using System.Collections;

public class PlayerAirDropHandler : MonoBehaviour
{
    public AirDropItemLootVFX AirDropItemLootVFX;

    private AirDropItemBase _airDropItem;
    public AirDropItemBase AirDropItem => _airDropItem;

    private Player _player;

    private void Awake()
    {
        if (_player == null)
        {
            _player = GetComponent<Player>();
        }
    }

    private void OnEnable()
    {
        if(_airDropItem == null)
        {
            return;
        }

        if(AirDropItemLootVFX.IsSelected)
        {
            AirDropItemLootVFX.UseItem();
            _airDropItem.Use();
            RemoveAirDropItem();
            return;
        }

        AirDropItemLootVFX.StartRoulette(_airDropItem);
        StartCoroutine(AirDropItemUseCoroutine());
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public void SetAirDropItem(AirDropItemBase airDropItem)
    {
        _airDropItem = Instantiate(airDropItem, transform);
        _airDropItem.SetOwner(_player);
        AirDropItemLootVFX.StartRoulette(_airDropItem);

        StartCoroutine(AirDropItemUseCoroutine());
    }

    private IEnumerator AirDropItemUseCoroutine()
    {
        yield return new WaitForSeconds(2.5f);
        if(_airDropItem != null)
        {
            AirDropItemLootVFX.UseItem();
            _airDropItem.Use();
            RemoveAirDropItem();
        }
    }

    public void RemoveAirDropItem()
    {
        Destroy(_airDropItem.gameObject);
        _airDropItem = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
