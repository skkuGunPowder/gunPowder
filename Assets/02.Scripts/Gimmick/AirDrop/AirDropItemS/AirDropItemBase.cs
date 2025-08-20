using UnityEngine;

public class AirDropItemBase : MonoBehaviour, IAirDropItem
{
    protected Player _owner;
    public Sprite Icon;

    public void SetOwner(Player player)
    {
        _owner = player;
    }

    public virtual void Use()
    {

    }
}
