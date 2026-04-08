using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GimmickManager : PhotonSingleton<GimmickManager>
{
    private Dictionary<GimmickType, List<IGimmick>> _gimmicksByType = new Dictionary<GimmickType, List<IGimmick>>();
    private Dictionary<GimmickGroupType, List<IGimmick>> _gimmicksByGroup = new Dictionary<GimmickGroupType, List<IGimmick>>();

    private void Start()
    {
        EventManager.Instance.OnGameStart += OnGameStart;
    }

    private void OnGameStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Start 그룹만 게임 시작 시 전부 활성화
        // During 그룹은 외부에서 수동 호출해야 활성화됨
        ActivateAllByGroup(GimmickGroupType.Start);
    }

    public void ActivateAllByGroup(GimmickGroupType group)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!_gimmicksByGroup.ContainsKey(group)) return;

        int count = _gimmicksByGroup[group].Count;
        if (count == 0) return;

        ActivateRandomByGroup(group, count);
    }

    public void Register(IGimmick gimmick)
    {
        if (!_gimmicksByType.ContainsKey(gimmick.Type))
            _gimmicksByType[gimmick.Type] = new List<IGimmick>();

        if (!_gimmicksByType[gimmick.Type].Contains(gimmick))
            _gimmicksByType[gimmick.Type].Add(gimmick);

        if (!_gimmicksByGroup.ContainsKey(gimmick.GroupType))
            _gimmicksByGroup[gimmick.GroupType] = new List<IGimmick>();

        if (!_gimmicksByGroup[gimmick.GroupType].Contains(gimmick))
            _gimmicksByGroup[gimmick.GroupType].Add(gimmick);
    }

    public void Unregister(IGimmick gimmick)
    {
        if (_gimmicksByType.ContainsKey(gimmick.Type))
            _gimmicksByType[gimmick.Type].Remove(gimmick);

        if (_gimmicksByGroup.ContainsKey(gimmick.GroupType))
            _gimmicksByGroup[gimmick.GroupType].Remove(gimmick);
    }

    public void ActivateByType(GimmickType type)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_ActivateByType), RpcTarget.All, (int)type);
    }

    public void ActivateRandomByGroup(GimmickGroupType group, int count = 1)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!_gimmicksByGroup.ContainsKey(group)) return;

        List<IGimmick> candidates = _gimmicksByGroup[group];
        if (candidates.Count == 0) return;

        count = Mathf.Min(count, candidates.Count);

        List<int> allIndices = new List<int>();
        for (int i = 0; i < candidates.Count; i++)
            allIndices.Add(i);

        int[] selectedIndices = new int[count];
        for (int i = 0; i < count; i++)
        {
            int randomPos = Random.Range(0, allIndices.Count);
            selectedIndices[i] = allIndices[randomPos];
            allIndices.RemoveAt(randomPos);
        }

        photonView.RPC(nameof(RPC_ActivateByGroupIndices), RpcTarget.All, (int)group, selectedIndices);
    }

    public void DeactivateByType(GimmickType type)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_DeactivateByType), RpcTarget.All, (int)type);
    }

    public void DeactivateAll()
    {
        foreach (var pair in _gimmicksByType)
        {
            foreach (IGimmick gimmick in pair.Value)
            {
                if (gimmick.IsActive)
                    gimmick.Deactivate();
            }
        }
    }

    [PunRPC]
    private void RPC_ActivateByType(int typeInt)
    {
        GimmickType type = (GimmickType)typeInt;
        if (!_gimmicksByType.ContainsKey(type)) return;

        foreach (IGimmick gimmick in _gimmicksByType[type])
        {
            gimmick.Activate();
        }
    }

    [PunRPC]
    private void RPC_ActivateByGroupIndices(int groupInt, int[] indices)
    {
        GimmickGroupType group = (GimmickGroupType)groupInt;
        if (!_gimmicksByGroup.ContainsKey(group)) return;

        List<IGimmick> candidates = _gimmicksByGroup[group];
        foreach (int index in indices)
        {
            if (index >= 0 && index < candidates.Count)
            {
                candidates[index].Activate();
            }
        }
    }

    [PunRPC]
    private void RPC_DeactivateByType(int typeInt)
    {
        GimmickType type = (GimmickType)typeInt;
        if (!_gimmicksByType.ContainsKey(type)) return;

        foreach (IGimmick gimmick in _gimmicksByType[type])
        {
            if (gimmick.IsActive)
                gimmick.Deactivate();
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        DeactivateAll();

        if (EventManager.Instance != null)
            EventManager.Instance.OnGameStart -= OnGameStart;
    }
}
