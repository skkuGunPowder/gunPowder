using UnityEngine;
using UnityEngine.UI;

public class UI_MainCategorySlot : MonoBehaviour, ISelectable
{
    public EMainCategory MainCategory;
    public Image BackgroundImage;
    public Sprite SelectedSprite;
    public Sprite UnselectedSprite;
    public GameObject SelectedBackground;
    

    public void Select()
    {
        BackgroundImage.sprite = SelectedSprite;
        SelectedBackground.SetActive(true);
    }

    public void Deselect()
    {
        BackgroundImage.sprite = UnselectedSprite;
        SelectedBackground.SetActive(false);
    }
    
     public void OnClick()
    {
        ItemStorage.Instance.ChangeMainCategory(MainCategory);
    }
}
