using UnityEngine;
using UnityEngine.UI;

public class UI_MainCategorySlot : MonoBehaviour, ISelectable
{
    public EMainCategory MainCategory;
    public Image BackgroundImage;
    public Sprite SelectedSprite;
    public Sprite UnselectedSprite;
    

    public void Select()
    {
        BackgroundImage.sprite = SelectedSprite;
    }

    public void Deselect()
    {
        BackgroundImage.sprite = UnselectedSprite;
    }
    
     public void OnClick()
    {
        ItemStorage.Instance.ChangeMainCategory(MainCategory);
    }
}
