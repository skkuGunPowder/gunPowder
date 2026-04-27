using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartridgeGoods : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _explanation;
    [SerializeField] private TextMeshProUGUI _price;

    public void Refresh(CartridgeData data)
    {
        Debug.Log($"{data.Name} CartridgeGoods Refresh");
        _iconImage.sprite = data.ImageSprite;
        _title.text = data.Name;
        _explanation.text = data.Explanation;
        _price.text = $"{data.GetPrice().ToString()} GP";
    }
}
