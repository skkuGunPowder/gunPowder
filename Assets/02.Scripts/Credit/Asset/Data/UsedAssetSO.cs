 using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UsedAssetSO", menuName = "Scriptable Objects/UsedAssetSO")]
public class UsedAssetSO : ScriptableObject
{
    [System.Serializable]
    public class AssetCredit
    {
        public string Creator;     // 제작자
        public string Usage;       // 사용처
        public string AssetName;   // 에셋 이름
    }

    public List<AssetCredit> Credits = new();  // 묶음 리스트
}
