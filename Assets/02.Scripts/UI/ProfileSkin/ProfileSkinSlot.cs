using UnityEngine;
using UnityEngine.UI;

public class ProfileSkinSlot : MonoBehaviour
{
    //스킨 타입에 따른 이미지 넣어주기 위함
    public EItemType SkinSlotType;
    public Image SkinImage;
    public Image DefaultImage;
    
    // 받은 이미지 적용시키기
    public void Refresh(Sprite skinImage = null)
    {
        if (skinImage == null)
        {
            DefaultImage.enabled = true;
            SkinImage.enabled = false;
        }
        else
        {
            DefaultImage.enabled = false;
            SkinImage.enabled = true;
            SkinImage.sprite = skinImage;
        }
    }
    
}
