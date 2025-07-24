using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public List<Transform> SpawnPoints = new List<Transform>();

    public void GeneratePlayers(int count, int gunpowder, int life)
    {
        var player = PhotonNetwork.Instantiate("PlayerTest", SpawnPoints[count].position, Quaternion.identity, 0);

        if (player.GetComponent<Player>().PhotonView.IsMine)
        {
            player.tag = "Player";

            ProCamera2D proCamera = Camera.main.GetComponent<ProCamera2D>();
            if (proCamera.CameraTargets.Count == 0)
            {
                proCamera.AddCameraTarget(player.transform);
            }
        }
        else
        {
            player.tag = "enemy";
        }
        
        player.GetComponent<PlayerStat>().SetPlayer(gunpowder, life);

    }
}
