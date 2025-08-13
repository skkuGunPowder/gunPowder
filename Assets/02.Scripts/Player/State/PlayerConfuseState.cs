using UnityEngine;

public class PlayerConfuseState : PlayerBaseState
{
    private float _timer = 0f;
    private float _directionChangeTimer = 0f;
    private float _directionChangeIntervalMin = 1f;
    private float _directionChangeIntervalMax = 4f;
    private float _directionChangeInterval = 0f;
    private float _direction = 1f; // -1 왼쪽 1 오른쪽
    [SerializeField] private float _minChangeCooldown = 0.25f; // 방향 전환 최소 간격
    [SerializeField] private float _speedEpsilon = 0.05f;      // 벽 등에 막혔다고 판단하는 속도 임계값
    private float _sinceLastChange = 0f;
    public override void OnEnter()
    {
        base.OnEnter();
        _timer = 0f;
        _directionChangeTimer = 0f;
        _directionChangeInterval = Random.Range(_directionChangeIntervalMin, _directionChangeIntervalMax);
        _sinceLastChange = 0f;

        // 이동 상태값 설정 (중력은 그대로 유지)
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.RunSpeed;
        _direction = _owner.PlayerStat.FacingDirection >= 0 ? 1f : -1f;

        _owner.ResetAnimatorTrigger("Idle");
        _owner.RPC_SetAnimatorTrigger("Confuse");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.RPC_ResetAnimatorTrigger("Confuse");
    }

    public override void MineUpdate()
    {
        _timer += Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;
        _sinceLastChange += Time.deltaTime;

        if(_timer >= _owner.PlayerStat.ConfuseTime)
        {
            SyncStateChange<PlayerIdleState>();
        }

        float xVelocity = _owner.Rigidbody2D.linearVelocity.x;
        if(_sinceLastChange >= _minChangeCooldown && Mathf.Abs(xVelocity) <= _speedEpsilon)
        {
            _directionChangeTimer = 0f;
            _sinceLastChange = 0f;
            _direction = _direction == 1 ? -1 : 1;
            _owner.RPC_SetFacingDirection(_direction > 0 ? 1 : -1);
        }

        // 방향 변경 (무작위 간격)
        if (_directionChangeTimer >= _directionChangeInterval)
        {
            _directionChangeTimer = 0f;
            _sinceLastChange = 0f;
            _directionChangeInterval = Random.Range(_directionChangeIntervalMin, _directionChangeIntervalMax);
            _direction = Random.value < 0.5f ? -1f : 1f;
            _owner.RPC_SetFacingDirection(_direction > 0 ? 1 : -1);
        }

        // 좌우 이동: 수평 속도만 변경, 수직 속도는 그대로 둬서 중력 적용 유지
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _owner.PlayerStat.RunSpeed * _direction;
        _owner.Rigidbody2D.linearVelocity = velocity;

    }
}
