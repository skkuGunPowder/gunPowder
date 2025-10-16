using TMPro;
using UnityEngine;

public class SpecialThanksSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text[] _nameTexts; // 3개 (1, 2, 3)

    /// <summary>
    /// 슬롯에 최대 3명의 이름을 표시
    /// </summary>
    public void SetNames(string[] names)
    {
        for (int i = 0; i < _nameTexts.Length; i++)
        {
            if (i < names.Length && !string.IsNullOrEmpty(names[i]))
            {
                _nameTexts[i].gameObject.SetActive(true);
                _nameTexts[i].text = names[i];
            }
            else
            {
                _nameTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
