using UnityEngine;

public class MissileBomb : MonoBehaviour, IBomb
{
    private string ID;
    private BombStat bombStat;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        bombStat = ItemDatabase.Instance.GetStat<BombStat>(ID);
    }



    public void PlaceBomb(Transform transform)
    {

    }

    public void ThrowBomb(Transform transform)
    {

    }

    public void ThrowBombStraight(Transform transform)
    {

    }

    public void BoostBomb(Transform transform)
    {

    }

    public void SmashBomb(Transform transform)
    {

    }

    public void SetLastBombTime()
    {

    }

    public void TakeDamage()
    {

    }
}
