using UnityEngine;

public class UI_Category : MonoBehaviour
{
    public EEquipmentSlot Category;

    public void OnClick()
    {
        ItemStorage.Instance.ChangeCategory(Category);
    }
}
