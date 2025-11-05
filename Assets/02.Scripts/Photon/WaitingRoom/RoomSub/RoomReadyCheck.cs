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

        int team = 0;
        EInGameTeam masterTeam = RoomManager.Instance.SelectedTeam;
        
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

            if (player.CustomProperties.ContainsKey(EProperties.Team.ToString()) == false) // 팀 체크
            {
                UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
                popup.Init("다시 시도해주세요", false);
                return false;
            }
            
            int other = (int)player.CustomProperties[EProperties.Team.ToString()];
            if (other != (int)masterTeam) // 모두 같은 팀인지 체크
            {
                team++;
            }
        }

        if (team == 0)
        {
            UI_MessagePopup popup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
            popup.Init("모두가 같은 팀입니다.", false);
            return false; //다른 팀 없음
        }

        return true;
    }
}
