using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class UI_PingBase : MonoBehaviour
{
    public static UI_PingBase Instance;

    public List<UI_Ping> PingList;
    public int Margin = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var ping in PingList)
        {
            ping.Content.SetActive(false);
        }

        EventManager.Instance.OnPlayerEntered += SetPing;
    }

    public void SetPing()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            PingList[i].SetPlayerTransform(enemies[i].transform);
        }
    }
}
