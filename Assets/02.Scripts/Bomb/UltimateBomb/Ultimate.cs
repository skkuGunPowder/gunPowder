using Photon.Pun;
using UnityEngine;
public abstract class Ultimate : MonoBehaviour
{
    protected string _ownerBombID;
    protected Player _owner;
    protected BombStat _bombStat;

    private void Awake()
    {
        Init();
    }
    
    public virtual void Init()
    {

    }

    public void SetOwner(Player owner)
    {
        _owner = owner;
    }

    public string GetBombID()
    {
        if (string.IsNullOrEmpty(_ownerBombID))
        {
            Debug.LogError("폭탄 ID를 확인 할 수 없습니다.");
            return null;
        }
        return _ownerBombID;
    }

    public virtual void ExcuteUltimate()
    { 
    }

    public int GetCost()
    {
        return _bombStat.Cost;
    }
}