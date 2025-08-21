using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class PlayerCountChecker : MonoBehaviour
{
    private List<PhotonPlayer> _playerList;
    public List<UI_PlayerCountSetup> StartProductionList;
    private void Awake()
    {
        _playerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
    }
    // UI전부 다 끄기
    private void OnEnable()
    {
        foreach (UI_PlayerCountSetup setup in StartProductionList)
        {
            setup.productionUI.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        SetupUIForPlayerCount(_playerList.Count);
    }

    // 플레이어 수와 컨텐트 셋업의 숫자가 같은지 체크
    public void SetupUIForPlayerCount(int playerCount)
    {
        UI_PlayerCountSetup matchingSetup = null;
        
        foreach (UI_PlayerCountSetup setup in StartProductionList)
        {
            if (setup.playerCount == playerCount)
            {
                matchingSetup = setup;
                break;
            }
        }

        if (matchingSetup != null && matchingSetup.productionUI != null)
        {
            matchingSetup.productionUI.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("아직 셋업이 되지 않았거나 셋업 중 오류가 발생했습니다.");
        }
    }

}
