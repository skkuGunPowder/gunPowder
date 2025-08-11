using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public List<Transform> SpawnPoints = new List<Transform>();
    
    public void GeneratePlayers(int count)
    {
        GameObject playerInstance = PhotonNetwork.Instantiate("PlayerTest", SpawnPoints[count].position, Quaternion.identity, 0);
        Player player = playerInstance.GetComponent<Player>();

        if (player.PhotonView.IsMine)
        {
            player.tag = "Player";

            CameraController proCamera = Camera.main.GetComponent<CameraController>();
            proCamera.SetTarget(player);
            UltimateManager.Instance.SetPlayer(player);
        }
        else
        {
            player.tag = "Enemy";
        }
    }
}
