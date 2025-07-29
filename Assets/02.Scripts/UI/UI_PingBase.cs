using System.Collections.Generic;
using UnityEngine;

public class UI_PingBase : MonoBehaviour
{
    public static UI_PingBase Instance;

    public List<UI_Ping> PingList;
    public List<Camera> RenderTexCameraList;
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
    }

    public void SetPing(Transform playerTransform)
    {
        int index = 0;
        foreach (var ping in PingList)
        {
            if (ping.PlayerTransform == null)
            {
                ping.SetPlayerTransform(playerTransform);
                ping.SetRenderTexCamera(RenderTexCameraList[index]);
                return;
            }
            ++index;
        }
        Debug.LogWarning("남아있는 플레이어 핑이 없습니다.");
    }
}
