using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;

    public GameObject DieExplosionPrefab;

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {   
        _timer += Time.deltaTime;
        // 플레이어가 사망할 떄, 사망 폭발이 발생

        // 부활
        // 목숨이 존재하는 규칙일 경우, 폭발한 플레이어는 100의 건파우더를 갖고 몇 초 후 부활한다.
        //    ㄴ PlayerSettingManager에서 받아올 예정
        // 부활 위치에 부활하기전 미리 알려주는 이펙트 발생
        

        // 부활
        

    }
}
