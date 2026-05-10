using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ThemeButton : MonoBehaviour
{
    public UI_MapSelectPopup UI_MapSelectPopup;
    public EMapTheme Theme;
    public TextMeshProUGUI ThemeNameText;
    public Image ThemeImage;
    public GameObject IsSelected;
    public void Refresh(EMapTheme theme, Sprite themeImage, string themeName)
    {
        Theme = theme;
        ThemeImage.sprite = themeImage;
        ThemeNameText.text = TextManager.Instance.GetText(themeName);
    }

    public void SelectCheck(EMapTheme theme)
    {
        IsSelected.SetActive(theme == Theme);
    }
    public void OnClickTheme()
    {
        UI_MapSelectPopup.ChangeTheme(Theme);
    }
}
