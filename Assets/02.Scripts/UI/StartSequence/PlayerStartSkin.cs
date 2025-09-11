using System;
using System.Collections.Generic;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class PlayerStartSkin : MonoBehaviour
{
    public Dictionary<EItemType, ItemDTO> EquipedItemDict = new Dictionary <EItemType,ItemDTO>();
    public IPlayerSkinManager _skinManager;

    private void Awake()
    {
        _skinManager = GetComponent<IPlayerSkinManager>();
    }

    public void Refresh(PhotonPlayer player)
    {
        LoadItems(player);
    }
    
    private void LoadItems(PhotonPlayer player)
    {
        for (int i = 0; i < (int)EItemType.None; i++)
        {
            EItemType itemType = (EItemType)i;
            if (player.CustomProperties.TryGetValue(itemType.ToString(), out object itemID))
            {
                if (EquipedItemDict.ContainsKey((EItemType)i))
                {
                    EquipedItemDict[(EItemType)i] = ItemDatabase.Instance.GetItem((string)itemID);
                }
                else
                {
                    EquipedItemDict.Add((EItemType)i, ItemDatabase.Instance.GetItem((string)itemID));
                }
            }
            else
            {
                // 해당 슬롯이 해제되었거나 값이 제거된 경우 로컬 딕셔너리에서도 제거
                if (EquipedItemDict.ContainsKey(itemType))
                {
                    EquipedItemDict.Remove(itemType);
                }
            }
        }

        foreach (var VARIABLE in EquipedItemDict)
        {
            Debug.Log($"{VARIABLE.Key} : {VARIABLE.Value}");
        }
        // [스킨] 단순 존재 여부 기반 적용/해제: 장착되었으면 적용, 없으면 해제
        ItemDTO headItem = null;
        ItemDTO faceItem = null;
        ItemDTO chestItem = null;
        ItemDTO capeItem = null;

        EquipedItemDict.TryGetValue(EItemType.Head, out headItem);
        EquipedItemDict.TryGetValue(EItemType.Face, out faceItem);
        EquipedItemDict.TryGetValue(EItemType.Chest, out chestItem);
        EquipedItemDict.TryGetValue(EItemType.Cape, out capeItem);

        if (headItem != null) { ApplyHeadSkin(headItem); } else { ClearHeadSkin(); }
        if (faceItem != null) { ApplyFaceSkin(faceItem); } else { ClearFaceSkin(); }
        if (chestItem != null) { ApplyChestSkin(chestItem); } else { ClearChestSkin(); }
        if (capeItem != null) { ApplyCapeSkin(capeItem); } else { ClearCapeSkin(); }

    }
    
    private void ApplyHeadSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyHead(item); }
    }
    private void ClearHeadSkin()
    {
        if (_skinManager != null) { _skinManager.ClearHead(); }
    }
    private void ApplyFaceSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyFace(item); }
    }
    private void ClearFaceSkin()
    {
        if (_skinManager != null) { _skinManager.ClearFace(); }
    }
    private void ApplyChestSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyChest(item); }
    }
    private void ClearChestSkin()
    {
        if (_skinManager != null) { _skinManager.ClearChest(); }
    }
    private void ApplyCapeSkin(ItemDTO item)
    {
        if (_skinManager != null) { _skinManager.ApplyCape(item); }
    }
    private void ClearCapeSkin()
    {
        if (_skinManager != null) { _skinManager.ClearCape(); }
    }
}
