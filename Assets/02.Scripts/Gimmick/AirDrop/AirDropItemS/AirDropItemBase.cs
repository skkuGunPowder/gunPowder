using UnityEngine;

public abstract class AirDropItemBase : MonoBehaviour, IAirDropItem
{
    protected Player _owner;
    public Sprite Icon;

    public void SetOwner(Player player)
    {
        _owner = player;
    }

    public virtual void Use()
    {
        if (_owner == null)
        {
            Debug.LogError("아이템 사용자가 없습니다.");
            return;
        }
    }
}
