using TMPro;
using UnityEngine;

public class UsedAssetSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text CreatorText;
    [SerializeField] private TMP_Text UsageText;
    [SerializeField] private TMP_Text AssetNameText;

    /// <summary>
    /// SO 데이터 한 묶음을 슬롯에 표시
    /// </summary>
    public void SetData(UsedAssetSO.AssetCredit credit)
    {
        if (credit == null) return;

        if (CreatorText) CreatorText.text = credit.Creator;
        if (UsageText) UsageText.text = credit.Usage;
        if (AssetNameText) AssetNameText.text = credit.AssetName;
    }
}
