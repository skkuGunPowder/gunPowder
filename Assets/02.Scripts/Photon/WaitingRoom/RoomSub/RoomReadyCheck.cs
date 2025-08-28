using Photon.Realtime;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

public class RoomReadyCheck
{
    private float _minimumPlayerCount = 2;
    public bool IsPlayerReady()
    {
        Room room = PhotonNetwork.CurrentRoom;
        
        if (room.PlayerCount < _minimumPlayerCount)
        {
            UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
            popup.Init("다른 플레이어가 없습니다.", false);
            return false;
        }

        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        foreach (PhotonPlayer player in players)
        {
            if (player.IsMasterClient)
            {
                continue;
            }

            if (player.CustomProperties.ContainsKey(EProperties.IsReady.ToString()) == false || (bool)player.CustomProperties[EProperties.IsReady.ToString()] == false)
            {
                UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
                popup.Init("모든 플레이어가 준비되지 않았습니다.", false);
                return false;
            }
        }

        return true;
    }
}
