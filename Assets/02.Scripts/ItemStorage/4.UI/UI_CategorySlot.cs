using UnityEngine;

public class UI_CategorySlot : MonoBehaviour, ISelectable
{
    public EItemType Category;
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
        ItemStorage.Instance.ChangeCategory(Category);
    }
}
