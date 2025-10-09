using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_ProfileSlot : MonoBehaviour
{
    public TextMeshProUGUI NicknameTextUGUI;
    
    public GameObject Ready;
    public GameObject NotReady;
    public GameObject Master;

    public GameObject Lives;
    public List<GameObject> LifeList = new List<GameObject>();
    public Image BombImage;
    public Image ProfileOutline;
    
    public ProfileSkin PlayerProfileSkin;
    public Sprite EmptyImage;
    public ColorPalette ColorPalette;
    
    // 후에 프로필 이미지 추가하기
    public void Refresh(PhotonPlayer player = null)
    {
        if (player == null)
        {
            NoPlayer();
            return;
        }
        
        NicknameTextUGUI.gameObject.SetActive(true);   
        NicknameTextUGUI.text = player.NickName;
        // skincheck
        PlayerProfileSkin.gameObject.SetActive(true);
        PlayerProfileSkin.Init(player);
        
        // life
        LifeSet();
        MasterCheck(player.IsMasterClient);
        // bomb
        ItemDTO item = ItemDatabase.Instance.GetItem(player.CustomProperties[EItemType.Bomb.ToString()].ToString());
        BombImage.sprite = item.Image;
        
        TeamSet(player);
        
    }
    
    private void LifeSet()
    {
        Lives.SetActive(true);

        int life = (int)PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.Life.ToString()];

        for (int i = 0; i < LifeList.Count; i++)
        {
            if (i < life)
            {
                LifeList[i].SetActive(true);
            }
            else
            {
                LifeList[i].SetActive(false);
            }
        }
    }
    public void ReadyCheck(bool isReady)
    {
        Master.gameObject.SetActive(false);
        
        Ready.SetActive(isReady);
        NotReady.SetActive(!isReady);
    }
    
    public void MasterCheck(bool isMaster)
    {
        Master.gameObject.SetActive(isMaster);
        Ready.gameObject.SetActive(!isMaster);
        NotReady.gameObject.SetActive(!isMaster);
    }
    public void NoPlayer()
    {
        Ready.SetActive(false);
        NotReady.SetActive(false);
        Lives.SetActive(false);
        NicknameTextUGUI.gameObject.SetActive(false);
        Master.SetActive(false);
        ProfileOutline.color = TeamColorSet(EInGameTeam.Default);
        PlayerProfileSkin.gameObject.SetActive(false);
        BombImage.sprite = EmptyImage;
    }

    public void TeamSet(PhotonPlayer player)
    {
        EInGameTeam team = EInGameTeam.Red;
        
        if(player.CustomProperties.ContainsKey(EProperties.Team.ToString()))
        {
            int teamNumber = (int)player.CustomProperties[EProperties.Team.ToString()];
            team = (EInGameTeam)teamNumber;
        }
        
        ProfileOutline.color = TeamColorSet(team);
        PlayerProfileSkin.TeamChanged(team);
    }

    private Color32 TeamColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red:
                return ColorPalette.ColorDictionary[EColorType.Red];
            case EInGameTeam.Blue:
                return ColorPalette.ColorDictionary[EColorType.Blue];
            case EInGameTeam.Green:
                return ColorPalette.ColorDictionary[EColorType.Green];
            case EInGameTeam.Yellow:
                return ColorPalette.ColorDictionary[EColorType.Yellow];
            case EInGameTeam.Default:
                return ColorPalette.ColorDictionary[EColorType.White];
            default:
                return ColorPalette.ColorDictionary[EColorType.Red];
        }
    }
    
}
