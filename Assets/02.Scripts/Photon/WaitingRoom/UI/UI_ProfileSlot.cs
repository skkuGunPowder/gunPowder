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
    public Image ProfileImage;
    public Image BombImage;
    public Image ProfileOutline;
    
    public ProfileSkin PlayerProfileSkin;
    public UI_EmotionSlot Emotion;    
    public Sprite EmptyImage;
    public List<Color32> TeamColorCodeList;
    
    
    
    // 후에 프로필 이미지 추가하기
    public void Refresh(PhotonPlayer player = null)
    {
        if (player == null)
        {
            NoPlayer();
            return;
        }
        
        NicknameTextUGUI.text = player.NickName;
        // skincheck
        PlayerProfileSkin.gameObject.SetActive(true);
        PlayerProfileSkin.Init(player);
        
        // life
        LifeSet();
        
        // bomb
        ItemDTO item = ItemDatabase.Instance.GetItem(player.CustomProperties[EItemType.Bomb.ToString()].ToString());
        BombImage.sprite = item.Image;
        
        if(player.CustomProperties[EProperties.Team.ToString()] == null)
        {
            TeamSet(EInGameTeam.Red);
        }
        else
        {
            int teamNumber = (int)player.CustomProperties[EProperties.Team.ToString()];
            EInGameTeam team = (EInGameTeam)teamNumber;
            TeamSet(team);
        }
    }

    private void LifeSet()
    {
        Lives.SetActive(true);

        int life = (int)PhotonNetwork.CurrentRoom.CustomProperties[EProperties.Life.ToString()];

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
        
        NicknameTextUGUI.gameObject.SetActive(true);
        Master.gameObject.SetActive(false);
        
        if (isReady)
        {
            Ready.SetActive(true);
            NotReady.SetActive(false);
        }
        else
        {
            Ready.SetActive(false);
            NotReady.SetActive(true);
        }
    }

    public void MasterCheck(bool isMaster)
    {
        Master.gameObject.SetActive(isMaster);
        Ready.gameObject.SetActive(!isMaster);
        NotReady.gameObject.SetActive(!isMaster);
    }
    public void NoPlayer()
    {
        NotReady.SetActive(false);
        Lives.SetActive(false);
        Ready.SetActive(false);
        NicknameTextUGUI.gameObject.SetActive(false);
        Master.SetActive(false);
        ProfileOutline.color = TeamColorSet(EInGameTeam.Default);
        PlayerProfileSkin.gameObject.SetActive(false);
        BombImage.sprite = EmptyImage;
    }

    public void TeamSet(EInGameTeam team)
    {
        ProfileOutline.color = TeamColorSet(team);
        PlayerProfileSkin.TeamChanged(team);
    }

    public void Play(string emotion)
    {
        Emotion.Play(emotion);
    }
    private Color32 TeamColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red:
                return TeamColorCodeList[0];
            case EInGameTeam.Blue:
                return TeamColorCodeList[1];
            case EInGameTeam.Green:
                return TeamColorCodeList[2];
            case EInGameTeam.Yellow:
                return TeamColorCodeList[3];
            case EInGameTeam.Default:
                return TeamColorCodeList[4];
            default:
                return TeamColorCodeList[0];
        }
    }
    
}
