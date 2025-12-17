using UnityEngine;

/// <summary>
/// 게에 붙잡힌 상태이상 핸들러
/// PlayerCrabHoldedState의 로직을 이곳으로 이동
/// </summary>
public class CrabStatusHandler : IStatusEffectHandler
{
    public StatusEffectType StatusType => StatusEffectType.Crab;
    
    public void OnEnter(Player owner)
    {
        owner.RPC_SetAnimatorTrigger("HitLoop");
    }
    
    public void OnExit(Player owner)
    {
        // 게에 붙잡힌 상태 종료 시 필요한 정리 작업이 있다면 여기에 추가
    }
    
    public void Update(Player owner)
    {
        // 게에 붙잡힌 상태는 단순히 애니메이션만 재생하므로 업데이트 로직 없음
        // 필요시 여기에 추가 로직 구현 가능
    }
    
    public bool IsFinished(Player owner)
    {
        // 게에 붙잡힌 상태는 외부에서 종료 조건을 결정하므로 항상 false 반환
        // 실제 종료는 PlayerStatusState에서 외부 조건에 따라 처리
        return false;
    }
}

