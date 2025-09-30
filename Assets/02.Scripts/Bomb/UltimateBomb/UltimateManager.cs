using System.Collections.Generic;
using UnityEngine;

public class UltimateManager : Singleton<UltimateManager>
{
    public List<Ultimate> UltimateList;
    private Dictionary<string, Ultimate> _ultimateDict;
    private Player _player;

    protected override void Awake()
    {
        if (UltimateList == null || UltimateList.Count <= 0)
        {
            return;
        }

        _ultimateDict = new Dictionary<string, Ultimate>();
        foreach (Ultimate ultimate in UltimateList)
        {
            ultimate.Init();
            _ultimateDict.Add(ultimate.GetBombID(), ultimate);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U) && _player != null)
        {
            _player.Ultimate.ExcuteUltimate();
        }
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public Ultimate GetUltimate(string bombID, Player player)
    {
        if (_ultimateDict.TryGetValue(bombID, out Ultimate ultimate))
        {
            ultimate.SetOwner(player);
            return ultimate;
        }
        
        Debug.LogError($"[{bombID}]는 궁극기가 없습니다.");
        return null;
    }

    public T GetUltimate<T>(string bombID, Player player) where T : Ultimate
    {
        if (_ultimateDict.TryGetValue(bombID, out Ultimate ultimate))
        {
            ultimate.SetOwner(player);
            return ultimate as T;
        }

        Debug.LogError($"[{bombID}]는 궁극기가 없습니다.");
        return null;
    }
}
