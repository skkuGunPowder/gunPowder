using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartridgeGoods : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _explanation;
    [SerializeField] private TextMeshProUGUI _price;
    
     private CartridgeData _cartridgeData;
     
    public void Refresh(CartridgeData data)
    {
        _cartridgeData = data;
        
        // 카트리지 인벤토리에서 체크해서 할인
        _iconImage.sprite = data.ImageSprite;
        _title.text = data.Name;
        _explanation.text = data.Explanation;
        _price.text = $"{data.GetPrice().ToString()} GP";
    }

    public bool CanBuy()
    {
        int currentGP = RoomStatManager.Instance.PlayerGunpowder;
        int price = _cartridgeData.GetPrice();
        
        if (price < currentGP)
        {
            return false;
        }

        RoomStatManager.Instance.ChangeGunpowder(-price);
        return true;
    }
}
