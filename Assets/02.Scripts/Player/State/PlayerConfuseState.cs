using UnityEngine;

public class PlayerConfuseState : PlayerBaseState
{
    private float _timer = 0f;
    private float _directionChangeTimer = 0f;
    private float _directionChangeIntervalMin = 1f;
    private float _directionChangeIntervalMax = 4f;
    private float _directionChangeInterval = 0f;
    private float _direction = 1f; // -1 왼쪽 1 오른쪽
    public override void OnEnter()
    {
        base.OnEnter();
        _timer = 0f;
        _directionChangeTimer = 0f;
        _directionChangeInterval = Random.Range(_directionChangeIntervalMin, _directionChangeIntervalMax);

        // 이동 상태값 설정 (중력은 그대로 유지)
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
        _direction = _owner.PlayerStat.FacingDirection >= 0 ? 1f : -1f;
        _owner.RPC_SetAnimatorTrigger("Run");
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void MineUpdate()
    {
        _timer += Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        // 방향 변경 (무작위 간격)
        if (_directionChangeTimer >= _directionChangeInterval)
        {
            _directionChangeTimer = 0f;
            _directionChangeInterval = Random.Range(_directionChangeIntervalMin, _directionChangeIntervalMax);
            _direction = Random.value < 0.5f ? -1f : 1f;
        }

        // 좌우 이동: 수평 속도만 변경, 수직 속도는 그대로 둬서 중력 적용 유지
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.MyMoveSpeed * _direction;
        _owner.Rigidbody2D.linearVelocity = velocity;

        // 바라보는 방향 동기화
        _owner.RPC_SetFacingDirection(_direction > 0 ? 1 : -1);
    }
}
