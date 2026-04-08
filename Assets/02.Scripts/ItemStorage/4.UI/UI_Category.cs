using System.Collections.Generic;
using UnityEngine;

public enum EMainCategory
{
    Character,
    Bomb
}

public class UI_Category : MonoBehaviour
{
    public GameObject CharacterCategoryTab;
    public GameObject BombCategoryTab;
    [SerializeField] private EMainCategory _startCategory;
    [SerializeField] private List<UI_MainCategorySlot> _mainCategorieList;
    [SerializeField] private List<UI_CategorySlot> _subCategorieList;

    public void Refresh(EItemType currentCategory, EMainCategory currentMainCategory)
    {
        if (currentMainCategory == EMainCategory.Character)
        {
            CharacterCategoryTab.SetActive(true);
            BombCategoryTab.SetActive(false);
        }
        else
        {
            CharacterCategoryTab.SetActive(false);
            BombCategoryTab.SetActive(true);
        }

        foreach (UI_MainCategorySlot mainCategory in _mainCategorieList)
        {
            if (mainCategory.MainCategory == currentMainCategory)
            {
                mainCategory.Select();
                continue;
            }
            mainCategory.Deselect();
        }

        foreach (UI_CategorySlot category in _subCategorieList)
        {
            if (category.Category == currentCategory)
            {
                category.Select();
                continue;
            }
            category.Deselect();
        }
    }
}
