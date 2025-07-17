using UnityEngine;

public interface IBomb
{
    void PlaceBomb(Transform transform);
    void ThrowBomb(Transform transform);
    void ThrowBombStraight(Transform transform);
    void BoostBomb(Transform transform);
    void SmashBomb(Transform transform);
    void SetLastBombTime();
}
