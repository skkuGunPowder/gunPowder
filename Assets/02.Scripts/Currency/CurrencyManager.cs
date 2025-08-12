using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    public Gold PlayerGold { get; private set; }
    public Exp PlayerExp { get; private set; }

    private CurrencyRepository _repo;

    protected override void Awake()
    {
        base.Awake();

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
            _repo.SaveCurrencyData(PlayerGold, PlayerExp);
        }
    }

    public void AddGold(int amount)
    {
        PlayerGold.Add(amount);
        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
    }

    public Result SubtractGold(int amount)
    {
        if (!PlayerGold.Subtract(amount))
        {
            return new Result(false, "보유한 금액이 부족합니다.");
        }

        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
        return new Result(true);
    }

    public void AddExp(int value)
    {
        PlayerExp.Add(value);
        _repo.SaveCurrencyData(PlayerGold, PlayerExp);
    }
}
