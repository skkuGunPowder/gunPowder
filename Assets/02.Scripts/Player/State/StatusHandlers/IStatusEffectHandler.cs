using UnityEngine;

/// <summary>
/// 상태이상 핸들러 인터페이스
/// 각 상태이상별 로직을 처리하는 핸들러가 구현해야 하는 인터페이스
/// </summary>
public interface IStatusEffectHandler
{
    /// <summary>
    /// 상태이상 타입 반환
    /// </summary>
    StatusEffectType StatusType { get; }
    
    /// <summary>
    /// 상태이상 시작 시 호출
    /// </summary>
    void OnEnter(Player owner);
    
    /// <summary>
    /// 상태이상 종료 시 호출
    /// </summary>
    void OnExit(Player owner);
    
    /// <summary>
    /// 상태이상 업데이트 (로컬 플레이어에서만 호출)
    /// </summary>
    void Update(Player owner);
    
    /// <summary>
    /// 상태이상이 완료되었는지 확인
    /// </summary>
    bool IsFinished(Player owner);
}

