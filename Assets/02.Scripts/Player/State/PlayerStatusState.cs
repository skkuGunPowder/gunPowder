using UnityEngine;

/// <summary>
/// 플레이어 상태이상 통합 상태 클래스
/// 
/// 역할:
/// - 모든 상태이상(Confuse, Crab 등)을 하나의 상태로 통합 관리
/// - Strategy 패턴을 사용하여 각 상태이상별 핸들러로 로직 분리
/// - 향후 추가될 상태이상들을 쉽게 확장 가능한 구조
/// 
/// 동작 방식:
/// 1. 상태 진입 시 StatusEffectType에 따라 적절한 Handler 생성
/// 2. Handler의 OnEnter, Update, OnExit 메서드를 위임
/// 3. Handler가 IsFinished를 반환하면 Idle 상태로 전환
/// </summary>
public class PlayerStatusState : PlayerBaseState
{
    // 현재 활성화된 상태이상 핸들러
    private IStatusEffectHandler _currentHandler;
    
    // 현재 상태이상 타입 (네트워크 동기화용)
    private StatusEffectType _currentStatusType = StatusEffectType.None;
    
    // RPC 호출 전에 설정할 상태이상 타입 (정적 변수로 네트워크 동기화)
    private static StatusEffectType _pendingStatusType = StatusEffectType.None;
    
    /// <summary>
    /// 상태이상 타입 설정 (네트워크 동기화용, RPC 호출 전에 사용)
    /// </summary>
    public static void SetPendingStatusType(StatusEffectType statusType)
    {
        _pendingStatusType = statusType;
    }
    
    /// <summary>
    /// 상태이상 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        
        // 정적 변수에서 상태이상 타입 가져오기
        _currentStatusType = _pendingStatusType;
        _pendingStatusType = StatusEffectType.None; // 사용 후 초기화
        
        // StatusEffectType에 따라 적절한 Handler 생성
        _currentHandler = CreateHandler(_currentStatusType);
        
        if (_currentHandler == null)
        {
            Debug.LogError($"[PlayerStatusState] 알 수 없는 상태이상 타입: {_currentStatusType}");
            SyncStateChange<PlayerIdleState>();
            return;
        }
        
        // Handler의 OnEnter 호출
        _currentHandler.OnEnter(_owner);
    }
    
    /// <summary>
    /// 상태이상 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        
        if (_currentHandler != null)
        {
            _currentHandler.OnExit(_owner);
            _currentHandler = null;
        }
        
        _currentStatusType = StatusEffectType.None;
    }
    
    /// <summary>
    /// 상태이상 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        if (_currentHandler == null)
        {
            return;
        }
        
        // Handler의 Update 호출
        _currentHandler.Update(_owner);
        
        // Handler가 완료되었는지 확인
        if (_currentHandler.IsFinished(_owner))
        {
            SyncStateChange<PlayerIdleState>();
            return;
        }
    }
    
    /// <summary>
    /// StatusEffectType에 따라 적절한 Handler 생성
    /// </summary>
    private IStatusEffectHandler CreateHandler(StatusEffectType statusType)
    {
        switch (statusType)
        {
            case StatusEffectType.Confuse:
                return new ConfuseStatusHandler();
            case StatusEffectType.Crab:
                return new CrabStatusHandler();
            // 향후 추가될 상태이상들...
            default:
                return null;
        }
    }
}
