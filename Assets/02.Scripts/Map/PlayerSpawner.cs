using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public List<Transform> SpawnPoints = new List<Transform>();
    
    public void GeneratePlayers(int count)
    {
        PhotonNetwork.Instantiate("PlayerTest", SpawnPoints[count].position, Quaternion.identity, 0);
    }
}
