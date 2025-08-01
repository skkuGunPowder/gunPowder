using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class GameResultData
{
    public readonly PhotonPlayer Player;
    public readonly int Damage;
    public readonly int Kill;
    public readonly int SurviveTime;
    public readonly EInGameTeam Team;

    public float SurviveTimeRate;
    public float DamageRate;
    public float KillRate;
    public GameResultData(PhotonPlayer player, int damage, int kill, int surviveTime, EInGameTeam team)
    {
        Player = player;
        Damage = damage;
        Kill = kill;
        SurviveTime = surviveTime;
        Team = team;
    }

    public void CalculateSurviveTimeRate(int max)
    {
        SurviveTimeRate = (float)SurviveTime / max; 
    }

    public void CalculateDamageRate(int max)
    {
        DamageRate = (float)Damage / max;
    }

    public void CalculateKillRate(int max)
    {
        KillRate = (float)Kill / max;
    }
}
