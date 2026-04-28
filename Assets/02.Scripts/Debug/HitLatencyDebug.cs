using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격→폭발→피격→상호작용 사이의 지연을 ms 단위로 콘솔에 찍는 측정용 헬퍼.
/// 공격자(자신이 발사한 폭탄을 가진 클라) 기준으로 다음 4단계를 측정한다.
///   T0 : Bomb.Explode 실행 시점 (내가 본 폭발 순간)
///   +α : RPC_TakeDamage 도착 (적이 데미지 입었다는 신호 도착)
///   +β : SpawnAttackerHitParticles RPC 도착
///   +γ : 적중 파티클 실제 표시 (현재 0.05초 대기 끝난 직후)
/// 키는 attacker 의 PhotonView.ViewID 사용.
/// 효과 측정용이므로 릴리스 빌드에서는 Enabled = false 로 끄면 됨.
/// </summary>
public static class HitLatencyDebug
{
    public static bool Enabled = true;

    private static readonly Dictionary<int, double> _attackerExplodeTime = new Dictionary<int, double>();

    public static void MarkExplode(int attackerViewId)
    {
        if (!Enabled) return;
        _attackerExplodeTime[attackerViewId] = Time.realtimeSinceStartupAsDouble;
        Debug.Log($"[HitLatency] T0 Bomb.Explode (attacker {attackerViewId})");
    }

    public static void LogStage(int attackerViewId, string stage)
    {
        if (!Enabled) return;
        if (_attackerExplodeTime.TryGetValue(attackerViewId, out double t0))
        {
            double dtMs = (Time.realtimeSinceStartupAsDouble - t0) * 1000.0;
            Debug.Log($"[HitLatency] +{dtMs:F1}ms {stage} (attacker {attackerViewId})");
        }
    }
}
