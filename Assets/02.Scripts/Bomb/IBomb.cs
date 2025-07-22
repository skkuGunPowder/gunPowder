using UnityEngine;

public interface IBomb
{
    public void PlaceBomb(Transform transform);
    public void ThrowBomb(Transform transform);
    public void ThrowBombStraight(Transform transform);
    public void BoostBomb(Transform transform);
    public void SmashBomb(Transform transform);
}
