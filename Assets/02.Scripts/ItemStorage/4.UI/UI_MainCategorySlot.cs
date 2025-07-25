using UnityEngine;



public class UI_MainCategorySlot : MonoBehaviour, ISelectable
{
    public EMainCategory MainCategory;
    public GameObject SelectedIcon;

    private void Start()
    {
        SelectedIcon.SetActive(false);
    }

    public void Select()
    {
        SelectedIcon.SetActive(true);
    }

    public void Deselect()
    {
        SelectedIcon.SetActive(false);
    }
    
     public void OnClick()
    {
        ItemStorage.Instance.ChangeMainCategory(MainCategory);
    }
}
