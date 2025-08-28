using UnityEngine;
using System.Collections.Generic;


public class BuffManager : DontDestroySingleton<BuffManager>
{
    public List<Buff> BuffList { get; private set; }

    private Dictionary<string, Buff> _buffPrefabDict;

    private BuffRepository _repo;

    protected override void Awake()
    {
        base.Awake();

        _repo = new BuffRepository();
        _repo.OnStatLoaded += Init;
    }

    private void Init(Dictionary<string, BuffStat> buffStatDict)
    {
        _buffPrefabDict = new Dictionary<string, Buff>();

        foreach (Buff buff in BuffList)
        {
            if (_buffPrefabDict.ContainsKey(buff.ID))
            {
                continue;
            }

            if (buff.Stat == null)
            {
                buff.SetStat(buffStatDict[buff.ID]);
            }

            _buffPrefabDict.Add(buff.ID, buff);
        }
    }

    public Buff GetBuff(string buffID, Transform caller)
    {
        if (_buffPrefabDict.TryGetValue(buffID, out Buff buff))
        {
            return Instantiate(buff, caller);
        }

        Debug.LogWarning($"[{buff.ID}] 버프가 없습니다.");
        return null;
    }
}
