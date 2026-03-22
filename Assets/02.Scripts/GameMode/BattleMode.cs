using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleMode : GameModeBase
{
    /// <summary>
    /// 1. 플레이어 리스트 받아와서 소환하기
    /// </summary>
    
    protected override void Start()
    {
        base.Start();
        SetState();
    }

    private void SetState()
    {
        GameModeStateBase[] stateBases = this.GetComponents<GameModeStateBase>();

        foreach(GameModeStateBase mode in stateBases)
        {
            mode.Initialize(this);
            _stateDictionary.TryAdd(mode.State, mode);
        }
    }
    
    public override void GameStart()
    {
        Debug.Log($"GameStart - Mode : Spawn");
        SceneManager.UnloadSceneAsync(ESceneList.StartSequence.ToString()); // 연출씬 제거

        if (PhotonNetwork.IsMasterClient)
        {
            RequestStateChange(EModeState.Spawn);   
        }
    }

}
