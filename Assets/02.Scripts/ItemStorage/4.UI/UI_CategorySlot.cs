using UnityEngine;
using UnityEngine.UI;

public class UI_CategorySlot : MonoBehaviour, ISelectable
{
    public EItemType Category;
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
        ItemStorage.Instance.ChangeCategory(Category);
    }
}
