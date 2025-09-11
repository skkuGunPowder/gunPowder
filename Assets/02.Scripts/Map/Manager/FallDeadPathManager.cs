using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public struct FallDeadPathData
{
    [Header("X 좌표 범위")]
    [Tooltip("플레이어가 이 범위에 있을 때 이 경로를 사용합니다 MinX이상 MaxX미만")]
    public float MinX;
    [Tooltip("플레이어가 이 범위에 있을 때 이 경로를 사용합니다 MinX이상 MaxX미만")]
    public float MaxX;
    
    [Header("Transform 참조")]
    [Tooltip("낙사 시작 지점")]
    public Transform FallDeadStartPoint;
    [Tooltip("낙사 경로")]
    public Transform FallDeadPath;
    [Tooltip("부활 지점")]
    public Transform FallDeadEntPoint;
}

public class FallDeadPathManager : Singleton<FallDeadPathManager>
{
    [Header("플레이어 낙사 관련")]
    public List<FallDeadPathData> FallDeadPathDataList;

    public FallDeadPathData GetFallDeadPathData(float playerX)
    {
        // 안전성 체크: 리스트가 비어있거나 null인 경우
        if (FallDeadPathDataList == null || FallDeadPathDataList.Count == 0)
        {
            Debug.LogError("FallDeadPathDataList가 비어있습니다! FallDeadPathManager를 확인해주세요.");
            return new FallDeadPathData(); // 기본값 반환
        }

        // 범위에 맞는 데이터 찾기
        foreach (var data in FallDeadPathDataList)
        {
            // MinX < MaxX 검증
            if (data.MinX >= data.MaxX)
            {
                Debug.LogWarning($"잘못된 범위 설정: MinX({data.MinX}) >= MaxX({data.MaxX}). 이 데이터는 건너뜁니다.");
                continue;
            }

            if (playerX >= data.MinX && playerX < data.MaxX)
            {
                return data;
            }
        }
        return FallDeadPathDataList[0];
    }
}
