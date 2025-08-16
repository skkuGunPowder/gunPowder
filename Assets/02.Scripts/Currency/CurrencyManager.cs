using System;
using UnityEngine;

public enum ECurrencyType
{
    Gold,
    Diamond,
    EXP
}


public class CurrencyManager : DontDestroySingleton<CurrencyManager>
{
    public Diamond PlayerDiamond { get; private set; }
    public Gold PlayerGold { get; private set; }
    public Exp PlayerExp { get; private set; }

    private CurrencyRepository _repo;

    public event Action<int, int> OnDataChanged;

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
            PlayerDiamond = new Diamond(data.PlayerDiamondAmount);
            PlayerGold = new Gold(data.PlayerGoldAmount);
            PlayerExp = new Exp(data.PlayerExpAmount);
        }
        else
        {
            PlayerDiamond = new Diamond(0);
            PlayerGold = new Gold(0);
            PlayerExp = new Exp(0);
            _repo.SaveCurrencyData(PlayerDiamond, PlayerGold, PlayerExp);
        }
    }

    public void AddCurrency(ECurrencyType currencyType, int amount)
    {
        if (currencyType == ECurrencyType.Gold)
        {
            PlayerGold.Add(amount);
        }

        if (currencyType == ECurrencyType.Diamond)
        {
            PlayerDiamond.Add(amount);
        }

        if (currencyType == ECurrencyType.EXP)
        {
            PlayerExp.Add(amount);
        }

        _repo.SaveCurrencyData(PlayerDiamond, PlayerGold, PlayerExp);
        OnDataChanged?.Invoke(PlayerDiamond.GetAmount(), PlayerGold.GetAmount());
    }

    public Result SubtractCurrency(ECurrencyType currencyType, int amount)
    {
        if (currencyType == ECurrencyType.Gold)
        {
            if (!PlayerGold.Subtract(amount))
            {
                return new Result(false, "보유한 골드가 부족합니다.");
            }
        }

        if (currencyType == ECurrencyType.Diamond)
        {
            if (!PlayerDiamond.Subtract(amount))
            {
                return new Result(false, "보유한 다이아몬드가 부족합니다.");
            }
        }

        if (currencyType == ECurrencyType.EXP)
        {
            throw new Exception("EXP는 감소할 수 없습니다!");
        }

        _repo.SaveCurrencyData(PlayerDiamond, PlayerGold, PlayerExp);
        OnDataChanged?.Invoke(PlayerDiamond.GetAmount(), PlayerGold.GetAmount());

        return new Result(true);
    }
}
