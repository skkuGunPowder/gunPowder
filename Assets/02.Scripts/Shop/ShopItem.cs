using System;
using LitJson;


public enum EShopMainCategory
{
    Event,
    Bomb,
    Skin,
    Effect,
    Currency,
    ETC,
    None
}


public class ShopItem
{
    private string _id;
    public string ID => _id;
    private EShopMainCategory _mainCategory;
    public EShopMainCategory MainCategory => _mainCategory;
    private int _goldPrice;
    public int GoldPrice => _goldPrice;
    private int _diamondPrice;
    public int DiamondPrice => _diamondPrice;
    private int _cashPrice;
    public int CashPrice => _cashPrice;
    private int _maxAmount;
    public int MaxAmount => _maxAmount;
    private ItemDTO _itemInfo;
    public ItemDTO ItemInfo => _itemInfo;


    public ShopItem(JsonData json)
    {
        if (json == null)
        {
            throw new Exception("Json이 유효하지 않습니다");
        }

        _id = (string)json["GoodsID"];
        _mainCategory = (EShopMainCategory)Enum.Parse(typeof(EShopMainCategory), (string)json["Category"]);
        _goldPrice = int.Parse(json["GoldPowderPrice"].ToString());
        _diamondPrice = int.Parse(json["DiamondPowderPrice"].ToString());
        _cashPrice = int.Parse(json["CashPrice"].ToString());
        _maxAmount = int.Parse(json["MaxPurchaseLimit"].ToString());
        _itemInfo = ItemDatabase.Instance.GetItem(_id);
    }

    public ShopItem(string id, EShopMainCategory mainCategory, int goldPrice, int diamondPrice, int cashPrice, int amount)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("[ShopItem] ID가 없습니다.");
        }

        if (goldPrice < -1)
        {
            throw new Exception("[ShopItem] GoldPrice가 유효하지 않습니다.");
        }

        if (diamondPrice < -1)
        {
            throw new Exception("[ShopItem] DiamondPrice가 유효하지 않습니다.");
        }

        if (cashPrice < -1)

        {
            throw new Exception("[ShopItem] CashPrice가 유효하지 않습니다.");
        }

        if (amount < -1)
        {
            throw new Exception("[ShopItem] AvailableAmount가 유효하지 않습니다.");
        }


        _id = id;
        _mainCategory = mainCategory;
        _goldPrice = goldPrice;
        _diamondPrice = diamondPrice;
        _cashPrice = cashPrice;
        _maxAmount = amount;
        _itemInfo = ItemDatabase.Instance.GetItem(_id);
    }
}
