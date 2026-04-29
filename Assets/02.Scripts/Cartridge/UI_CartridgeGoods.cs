using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CartridgeGoods : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _explanation;
    [SerializeField] private TextMeshProUGUI _price;
    
    [SerializeField] private UI_Gradient _gradient;
    
     private CartridgeData _cartridgeData;

     private void Awake()
     {
         if (_gradient == null)
         {
             _gradient = GetComponentInChildren<UI_Gradient>();
         }
     }
     
    public void Refresh(CartridgeData data)
    {
        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _cartridgeData = data;
        
        // 카트리지 인벤토리에서 체크해서 할인
        _iconImage.sprite = data.ImageSprite;
        _title.text = data.Name;
        _explanation.text = data.Explanation;
        _price.text = $"{data.GetPrice().ToString()} GP";
        SetColor();
    }

    private void SetColor()
    {
        EColorType colorType = ColorPalette.GetColorTypeByName(_cartridgeData.Rarity.ToString());
        Color32[] colors = ColorPalette.GetGradationColors(colorType, 2);
        _gradient.Color1 = colors[0];
        _gradient.Color2 = colors[1];
    }
    public bool CanBuy()
    {
        int currentGP = RoomStatManager.Instance.PlayerGunpowder;
        int price = _cartridgeData.GetPrice();
        
        if (price > currentGP)
        {
            return false;
        }
        
        // 카트리지 적용
        if(CartridgeInventoryManager.Instance.AddCartridge(_cartridgeData.ID))
        {
            RoomStatManager.Instance.ChangeGunpowder(-price);
            return true;
        }
        return false;
    }
}
