using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class ProfileSkin : MonoBehaviour
{
    public List<ProfileSkinSlot> SkinSlotList;
    public List<ProfileTeamSlot> TeamSlotList;
    // 스킨 전체 바꾸기 : 첫 입장
    
    public void Init(PhotonPlayer player) 
    {
        foreach (ProfileSkinSlot slot in SkinSlotList)
        {
            Sprite item = FindItem(slot.SkinSlotType, player);
            slot.Refresh(item);
        }
    }
    // [추가] 채팅으로 받은 ID 리스트로 초기화 (로비/채팅용)
    public void Init(List<string> itemIds)
    {
        if (itemIds == null || itemIds.Count == 0) return;

        foreach (ProfileSkinSlot slot in SkinSlotList)
        {
            // 리스트에서 해당 슬롯 타입(Face, Hair 등)에 맞는 아이템을 찾아옴
            Sprite itemSprite = FindItemFromList(slot.SkinSlotType, itemIds);
            slot.Refresh(itemSprite);
        }
    }

    public void TeamChanged(EInGameTeam team)
    {
        foreach (ProfileTeamSlot slot in TeamSlotList)
        {
            slot.Refresh((int)team);
        }
    }
    // 부위 스킨 바꾸기
    public void Refresh(EItemType itemType, PhotonPlayer player)
    {
        foreach (ProfileSkinSlot slot in SkinSlotList)
        {
            if(slot.SkinSlotType == itemType)
            {
                Sprite item = FindItem(itemType, player);
                slot.Refresh(item);
                break;  
            }
        }
    }
    
    // 받아온 아이템 ID로 스프라이트 가져오기
    private Sprite FindItem(EItemType itemType,PhotonPlayer player)
    {
        if (player.CustomProperties.ContainsKey(itemType.ToString()) == false)
        {
            return null;    
        }
        
        ItemDTO item = ItemDatabase.Instance.GetItem(player.CustomProperties[itemType.ToString()].ToString());
        return item.SkinImage;
        
    }
    // [추가] ID 리스트(문자열)에서 찾기
    private Sprite FindItemFromList(EItemType itemType, List<string> itemIds)
    {
        foreach (string id in itemIds)
        {
            ItemDTO itemDTO = ItemDatabase.Instance.GetItem(id);
            // 아이템 DB에서 ID로 정보를 가져왔는데, 그게 지금 찾으려는 부위(예: Hair)가 맞다면 리턴
            if (itemDTO != null && itemDTO.ItemType == itemType)
            {
                return itemDTO.SkinImage;
            }
        }
        return null;
    }
}

