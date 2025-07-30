using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameResultData
{
    public readonly PhotonPlayer Player;
    public readonly int Damage;
    public readonly int Kill;
    public readonly int SurviveTime;
    public readonly EInGameTeam Team;

    public GameResultData(PhotonPlayer player, int damage, int kill, int surviveTime, EInGameTeam team)
    {
        Player = player;
        Damage = damage;
        Kill = kill;
        SurviveTime = surviveTime;
        Team = team;
    }
}
