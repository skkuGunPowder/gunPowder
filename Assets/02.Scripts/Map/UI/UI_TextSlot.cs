using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_TextSlot : MonoBehaviour
{
    /// <summary>
    /// 텍스트 하나만 변경할 경우 사용
    /// </summary>
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Image _backgroundImage;
    
    public void TextRefresh(string text)
    {
        Text.text = text;
    }

    public void TextRefresh(int text)
    {
        Text.text = text.ToString();
    }

    public void TextRefresh(float text)
    {
        Text.text = text.ToString();
    }
    
    public void BackgroundRefresh(Color32 color)
    {
        _backgroundImage.color = color;
    }
}
