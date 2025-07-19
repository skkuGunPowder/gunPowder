using UnityEngine;

public class UI_Category : MonoBehaviour, ISelectable
{
    public EEquipmentSlot Category;

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
