using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RepairSlot : MonoBehaviour
{
    private const string REPAIR_NAME = "수리";
    private const string DUABILITY_NAME = "남은 내구도";
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _durabilityText;
    [SerializeField] private TextMeshProUGUI _explanationText;
    // [SerializeField] private TextMeshProUGUI _repairCostText;
    [SerializeField] private Button _repairButton;

    private Cartridge _cartridge;
    // private Action _onRepaired;  // 수리 후 UI업데이트

    public void Refresh(Cartridge cartridge)
    {
        _cartridge = cartridge;

        // if (_iconImage != null)
        // {
        //     _iconImage.sprite = cartridge.Data.ImageSprite;
        // }
        _nameText.text = $"{REPAIR_NAME} : {cartridge.Data.Name}";
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 데이터 변수
        string _explanation = _cartridge.Data.Explanation;
        int currentDurability = _cartridge.GetCurrentDurability();
        int maxDurability = _cartridge.GetMaxDurability();
        int repairCost = _cartridge.Data.GetRepairCost(_cartridge.GetRepairCount());
        
        // 텍스트 수정
        _explanationText.text = _explanation;
        _durabilityText.text = $"{DUABILITY_NAME} : {currentDurability} ";
        // _repairCostText.text = $"{repairCost} GP";

        // 이미 수리했거나, 내구도가 최대이거나, 빚 상태면 버튼 비활성화
        bool canRepair = !_cartridge.IsRepairedThisTurn()
                         && currentDurability < maxDurability
                         && RoomStatManager.Instance.CanChangeGP(0);
        
        _repairButton.interactable = canRepair;
    }

    public void OnClickRepair()
    {
        if (CartridgeInventoryManager.Instance.TryRepairWithCost(_cartridge.Data.ID))
        {
            UpdateUI();
        }
    }
}
