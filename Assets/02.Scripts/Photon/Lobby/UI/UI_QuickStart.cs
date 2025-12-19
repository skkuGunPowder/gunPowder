using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_QuickStart : MonoBehaviour
{
    [SerializeField] private float _clickInterval = 0.5f;
    private bool _isClickInterval = false; // 쿨타임
    
    // 개인으로 만들어진 방 랜덤으로 들어가기
    public void OnClickQuickJoinRoom()
    {
        if (_isClickInterval)
        {
            return;
        }
        
        _isClickInterval = true;
       
        Task_ClickInterval().Forget();

        Hashtable hash = new Hashtable()
        {
            {ERoomProperties.IsLocked.ToString(), false}   
        };
        
        PhotonNetwork.JoinRandomRoom(hash, 0);
    }

    // 팀으로 만들어진 방 중 랜덤으로 들어가기
    public void OnClickPartyLoadScene()
    {
        // LobbyManager의 party라는 변수가 null이 아니라면 이 함수를 호출한다.
        // 
    }

    private async UniTaskVoid Task_ClickInterval()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        
        await UniTask.Delay(TimeSpan.FromSeconds(_clickInterval), cancellationToken: ct).SuppressCancellationThrow();
        
        _isClickInterval = false;
    }
}
