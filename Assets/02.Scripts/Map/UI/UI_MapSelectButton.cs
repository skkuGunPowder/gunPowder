using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapSelectButton : MonoBehaviour
{
    public UI_MapSetup UI_MapSetup;
    public GameObject IsSelected;
    public Image MapImage;
    public TextMeshProUGUI MapNameText;
    
    public void Refresh(EMap map, bool isSelected, Sprite mapImage, string mapName)
    {
        UI_MapSetup.SelectedMap = map;
        IsSelected.SetActive(isSelected);
        MapImage.sprite = mapImage;
        MapNameText.text = mapName;
    }
}
