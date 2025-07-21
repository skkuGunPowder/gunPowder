using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_ProfileSlot : MonoBehaviour
{
    public TextMeshProUGUI NicknameTextUGUI;
    public GameObject Ready;
    public GameObject NotReady;
    public Image ProfileImage;
     
    // 후에 프로필 이미지 추가하기
    public void Refresh(PhotonPlayer player = null)
    {
        if (player == null)
        {
            NoPlayer();
            return;
        }
        
        Debug.Log($"{player.ActorNumber}의 커스텀프로퍼티가 있나요? : {player.CustomProperties.ContainsKey(EProperties.IsReady)}");
        // ProfileImage.sprite = profileImage;
        // bool isReady = (bool)player.CustomProperties["isReady"];
        // ReadyCheck(isReady);
        //
        NicknameTextUGUI.text = player.NickName + player.ActorNumber;
        
    }

    public void ReadyCheck(bool isReady)
    {
        
        NicknameTextUGUI.gameObject.SetActive(true);
        
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

    public void NoPlayer()
    {
        NotReady.SetActive(false);
        Ready.SetActive(false);
        NicknameTextUGUI.gameObject.SetActive(false);
        
    }
}
