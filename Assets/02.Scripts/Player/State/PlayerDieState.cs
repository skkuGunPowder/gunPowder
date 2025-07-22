using Photon.Pun;
using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;

    public override void OnEnter()
    {
        base.OnEnter();

        // 플레이어가 사망할 떄, 사망 폭발이 발생
        //PhotonNetwork.Instantiate("DieExplosion", transform.position, Quaternion.identity);
        Debug.Log("죽음 폭발 발생");
    }

    public override void OnExit()
    {
        base.OnExit();

        // 플레이어 초기화
        _owner.InitializePlayer();
        
        //부활
    }

    public override void MineUpdate()
    {   
        _timer += Time.deltaTime;
        
        // 부활
        // 목숨이 존재하는 규칙일 경우, 폭발한 플레이어는 몇 초 후 부활한다.
        //    ㄴ PlayerSettingManager에서 받아올 예정
        
        // 부활 위치에 부활하기전 미리 알려주는 이펙트 발생
    }
}
