using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    /// <summary>
    /// 스폰 포인트 설정
    /// 플레이어 소환
    /// </summary>
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private List<Transform> SpawnPoints = new List<Transform>();
    [SerializeField] private List<RankSpawnPoint> RankSpawnPointList = new List<RankSpawnPoint>();
    
    /// <summary>
    /// 플레이어 소환
    /// 1. 순번에 따른 소환
    /// 2. 순위에 따른 소환
    /// 3. 팀별 소환
    /// </summary>
    
    // 번호에 따른 소환
    public GameObject GeneratePlayers(int count)
    {
        GameObject playerInstance = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoints[count].position, Quaternion.identity, 0);
        Player player = playerInstance.GetComponent<Player>();

        Debug.Log($"[Cartridge] PlayerSpawner before InvokeOnSpawned. player.ActorNumber={player.ActorNumber}, LocalPlayer.ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber}");
        PlayerEventManager.Instance.GetEvents(player.ActorNumber).InvokeOnSpawned();

        if (player.PhotonView.IsMine)
        {
            CameraController proCamera = Camera.main.GetComponent<CameraController>();
            proCamera.SetTarget(player);
            UltimateManager.Instance.SetPlayer(player);
        }
        
        return playerInstance;
    }

    // 순위에 따른 소환
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
            if (player != null)
            {
                PlayerEventManager.Instance.GetEvents(player.ActorNumber).InvokeOnSpawned();
            }

            break;
        }
    }
}
