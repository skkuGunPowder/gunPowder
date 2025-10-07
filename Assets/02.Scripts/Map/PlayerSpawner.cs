using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public List<Transform> SpawnPoints = new List<Transform>();
    public List<RankSpawnPoint> RankSpawnPointList = new List<RankSpawnPoint>();
    
    public void GeneratePlayers(int count)
    {
        GameObject playerInstance = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoints[count].position, Quaternion.identity, 0);
        Player player = playerInstance.GetComponent<Player>();

        if (player.PhotonView.IsMine)
        {
            CameraController proCamera = Camera.main.GetComponent<CameraController>();
            proCamera.SetTarget(player);
            UltimateManager.Instance.SetPlayer(player);
        }
    }

    public void GeneratePlayers(int rank, int spawnCount)
    {
        // 등수로 한번 구분
        foreach (RankSpawnPoint rankSpawnPoint in RankSpawnPointList)
        {
            if (rankSpawnPoint.Rank != rank)
            {
                continue;
            }
            
            GameObject playerInstance = PhotonNetwork.Instantiate(PlayerPrefab.name, rankSpawnPoint.SpawnPointList[spawnCount].position, Quaternion.identity, 0);
            Player player = playerInstance.GetComponent<Player>();
            break;
        }
        
    }
}
