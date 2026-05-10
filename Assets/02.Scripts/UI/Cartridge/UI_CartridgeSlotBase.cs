using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_CartridgeSlotBase : MonoBehaviour
{
    private const string DURABILITY_NAME = "남은 내구도";
    private const string COUNT_NAME = "보유 수량";

    [SerializeField] protected Image _iconImage;
    [SerializeField] protected TextMeshProUGUI _nameText;
    [SerializeField] protected TextMeshProUGUI _durabilityText;
    [SerializeField] protected TextMeshProUGUI _explanationText;
    [SerializeField] protected UI_Gradient _gradient;
    
    protected Cartridge _cartridge;

    private void Awake()
    {
        if (_gradient == null)
        {
            _gradient = GetComponentInChildren<UI_Gradient>();
        }
    }
    
    protected void RefreshBaseUI(Cartridge cartridge, string actionName)
    {
        _cartridge = cartridge;

        if (_iconImage != null)
        {
            _iconImage.sprite = cartridge.Data.ImageSprite;
        }

        SetColor(cartridge);
        _nameText.text = $"{actionName} : {cartridge.Data.Name}";
        _explanationText.text = cartridge.Data.Explanation;

        int currentDurability = cartridge.GetCurrentDurability();

        // 소모형(MaxDurability == 0)은 보유 수량, 영구형은 내구도 표시
        if (cartridge.GetMaxDurability() == 0)
        {
            _durabilityText.text = $"{COUNT_NAME} : {currentDurability}";
        }
        else
        {
            _durabilityText.text = $"{DURABILITY_NAME} : {currentDurability}";
        }
    }
    
    private void SetColor(Cartridge cartridge)
    {
        CartridgeData data = cartridge.Data; 
        EColorType colorType = ColorPalette.GetColorTypeByName(data.Rarity.ToString());
        Color32[] colors = ColorPalette.GetGradationColors(colorType, 2);
        _gradient.Color1 = colors[0];
        _gradient.Color2 = colors[1];
    }
}
