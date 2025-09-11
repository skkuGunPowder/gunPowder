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
        foreach (var data in FallDeadPathDataList)
        {
            if (playerX >= data.MinX && playerX < data.MaxX)
            {   
                return data;
            }
        }
        return FallDeadPathDataList[0];
    }
}
