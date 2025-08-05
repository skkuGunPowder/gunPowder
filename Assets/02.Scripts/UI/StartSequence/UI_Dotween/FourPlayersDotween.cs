using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
public class FourPlayersDotween : MonoBehaviour
{
    public RectTransform Player1;
    public RectTransform Player2;
    public RectTransform Player3;
    public RectTransform Player4;
    private void OnEnable()
    {
        Player1.DOAnchorPos(new Vector2(700, -540), 5);
        Player2.DOAnchorPos(new Vector2(700, -540), 5);
        Player4.DOAnchorPos(new Vector2(700, -540), 5);
        Player3.DOAnchorPos(new Vector2(700, -540), 5).OnComplete(() =>
        {
            Hashtable hash = new Hashtable()
            {
                {EProperties.IsLoad.ToString(), true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        });
    }
    
    private void OnDisable()
    {
        // 초기 위치로 리셋하기
    }
}
