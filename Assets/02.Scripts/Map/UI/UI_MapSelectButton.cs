using TMPro;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapSelectButton : MonoBehaviour
{
    public GameObject IsSelected;
    public Image MapImage;
    public TextMeshProUGUI MapNameText;
    public EMap Map;
    public void Refresh(EMap map, bool isSelected, Sprite mapImage, string mapName)
    {
        Map = map;
        IsSelected.SetActive(isSelected);
        MapImage.sprite = mapImage;
        MapNameText.text = mapName;
    }
    
    public void OnClickMapSelect()
    {
        Hashtable roomProperties = new Hashtable()
        {
            {ERoomProperties.MapSelected.ToString(), (int)Map}
        };
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
}
