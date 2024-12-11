using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeDishName;
    [SerializeField] private Image recipeDishIcon;
    [SerializeField] private Sprite recipeLockIcon;

    private int recipeTemplateId;
    private bool isDiscover = false;

    public void UpdateRecipeBookTemplateVisual(FinishDishSO menuDishSO)
    {
        if (RecipeBookManager.Instance.IsDiscoveredDish(menuDishSO))
        {
            recipeDishName.text = menuDishSO.finishDishName;
            recipeDishIcon.sprite = menuDishSO.finishDishIcon;
            recipeTemplateId = RecipeBookManager.Instance.GetFinishDishSOIndex(menuDishSO);
            isDiscover = true;
        }
        else
        {
            recipeDishName.text = "???";
            recipeDishIcon.sprite = recipeLockIcon;
        }
        
    }
    public void SetRecipeTemplateID(int recipeTemplateIndex)
    {
        //Debug.Log("SetRecipeTemplateID to " + recipeTemplateIndex);
        recipeTemplateId = recipeTemplateIndex;
    }

    public int GetRecipeTemplateId()
    {
        return recipeTemplateId;
    }

    public void SetUnlockRecipe()
    {
        recipeDishIcon.sprite = recipeLockIcon;
    }
    public bool IsDiscovered()
    {
        return isDiscover;
    }
}
