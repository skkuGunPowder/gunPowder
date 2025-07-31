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
    public Image ProfileImage;
     
    // 후에 프로필 이미지 추가하기
    public void Refresh(PhotonPlayer player = null)
    {
        if (player == null)
        {
            NoPlayer();
            return;
        }
        
        NicknameTextUGUI.text = player.CustomProperties[EProperties.NickName.ToString()].ToString();
        

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
        Ready.SetActive(false);
        NicknameTextUGUI.gameObject.SetActive(false);
        Master.SetActive(false);
        
    }
}
