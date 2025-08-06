using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public Gold PlayerGold { get; private set; }
    public Exp PlayerExp { get; private set; }

    private CurrencyRepository _repo;

    private void Awake()
    {
        _repo = new CurrencyRepository();
        Init();
    }

    private async void Init()
    {
        CurrencySaveData data = await _repo.LoadCurrencyData();
        if (data != null)
        {
            PlayerGold = new Gold(data.PlayerGoldAmount);
            PlayerExp = new Exp(data.PlayerExpAmount);
        }
        else
        {
            PlayerGold = new Gold(0);
            PlayerExp = new Exp(0);    
        }
    }

    public void AddGold(int amount)
    {
        PlayerGold.Add(amount);
        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
    }

    public void SubtractGold(int amount)
    {
        PlayerGold.Subtract(amount);
        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
    }

    public void AddExp(int value)
    {
        PlayerExp.Add(value);
        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
    }
}
