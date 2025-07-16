using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemDetail : MonoBehaviour
{
    [Header("텍스트")]
    public TextMeshProUGUI ItemNameText;
    public TextMeshProUGUI DescriptionText;

    [Header("아이템 스펙 패널")]
    public GameObject ItemSpecPanel;
    public Slider AttackPointSlider;
    public Slider CoastSlider;
    public Slider CoolTimeSlider;
    public Slider ExlposionRadiusSlider;

    [SerializeField] private ItemDTO _selectedItem;

    private void Start()
    {
        ItemStorage.Instance.OnDataChanged += Refresh;
        Init();
    }

    private void Init()
    {
        ItemNameText.text = "";
        DescriptionText.text = "";
        ItemSpecPanel.gameObject.SetActive(false);
    }

    public void Refresh(EEquipmentSlot equipmentSlot)
    {
        _selectedItem = ItemStorage.Instance.GetSelectedItem();

        if (_selectedItem == null)
        {
            Init();
            return;
        }

        if (_selectedItem.EquipmentSlot == EEquipmentSlot.Weapon)
        {
            ItemSpecPanel.gameObject.SetActive(true);
            // TODO
            // Bomb 도메인 불러와서 슬라이더에 값 전달
        }
        else
        {
            ItemSpecPanel.gameObject.SetActive(false);
        }

        ItemNameText.text = _selectedItem.Name;
        DescriptionText.text = _selectedItem.Description;
    }
}
