using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class ProfileSkin : MonoBehaviour
{
    public List<ProfileSkinSlot> SkinSlotList;
    
    // 스킨 전체 바꾸기 : 첫 입장
    public void Init(PhotonPlayer player) 
    {
        foreach (ProfileSkinSlot slot in SkinSlotList)
        {
            Sprite item = FindItem(slot.SkinSlotType, player);
            slot.Refresh(item);
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
        return item.Image;
        
    }
}
