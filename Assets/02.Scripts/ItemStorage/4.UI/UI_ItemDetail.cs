using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemDetail : MonoBehaviour
{
    [Header("텍스트")]
    public TextMeshProUGUI ItemNameText;
    public TextMeshProUGUI ExplanationText;

    [Header("아이템 스펙 패널")]
    public GameObject ItemSpecPanel;
    public TextMeshProUGUI AttackPointText;
    public TextMeshProUGUI StealPercentText;
    public TextMeshProUGUI CooltimeText;
    public Slider ExlposionRadiusSlider;

    [SerializeField] private InventoryItem _selectedItem;

    private void Start()
    {
        ItemStorage.Instance.OnDataChanged += Refresh;
        Init();
    }

    private void Init()
    {
        ItemNameText.text = "";
        ExplanationText.text = "";
        ItemSpecPanel.gameObject.SetActive(false);
    }

    public void Refresh(EItemType itemType)
    {
        _selectedItem = ItemStorage.Instance.GetSelectedItem();

        if (_selectedItem == null)
        {
            Init();
            return;
        }

        if (_selectedItem.Item.ItemType == EItemType.Bomb)
        {
            ItemSpecPanel.gameObject.SetActive(true);

            BombStat bombStat = ItemDatabase.Instance.GetStat<BombStat>(_selectedItem.ID);
            ExplosionStat explosionStat = ItemDatabase.Instance.GetStat<ExplosionStat>(bombStat.ExplosionID);

            AttackPointText.text = $"{explosionStat.AttackPower}";
            ExlposionRadiusSlider.value = explosionStat.ExplosionRadius;
            CooltimeText.text = $"{bombStat.CoolTime}s";
            StealPercentText.text = $"{explosionStat.StealPercent}%";
        }
        else
        {
            ItemSpecPanel.gameObject.SetActive(false);
        }

        ItemNameText.text = TextManager.Instance.GetText(_selectedItem.Item.Name);
        ExplanationText.text = TextManager.Instance.GetText(_selectedItem.Item.Explanation);
    }

    private void OnDestroy()
    {
        ItemStorage.Instance.OnDataChanged -= Refresh;
    }
}
