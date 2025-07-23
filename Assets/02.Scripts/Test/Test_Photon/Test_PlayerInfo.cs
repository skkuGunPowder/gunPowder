using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using RaycastPro.RaySensors2D;
using Unity.Mathematics;
using UnityEngine;

public class Test_PlayerInfo : MonoBehaviour
{

    public float timer;
    private bool _test = false;
    public ESceneList Scene;
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer == 5f)
        {
            Generate();
        }
    }

    public void Generate()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(0, 5, 0), quaternion.identity, 0);
    }

 
    public void OnClickChanged()
    {
        Hashtable load = new Hashtable()
        {
            { EProperties.IsLoad.ToString() , !_test }, 
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(load);
        
        _test = !_test;
        Debug.Log($"{load[EProperties.IsLoad.ToString()]}");
        Debug.Log("bool");
    }

    public void OnclickGameEnd()
    {
        Hashtable hash = new Hashtable()
        {
            { EProperties.IsDead.ToString(), true }
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void OnClickLoadScene()
    {
        PhotonNetwork.LoadLevel(Scene.ToString());
    }
}
