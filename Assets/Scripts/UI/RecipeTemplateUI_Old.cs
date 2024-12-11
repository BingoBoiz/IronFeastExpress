using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeTemplateUI_Old : MonoBehaviour
{

    [SerializeField] Transform RecipeIcon;
    [SerializeField] Transform RecipeLockIcon;
    [SerializeField] Transform RecipeLockBackground;

    private int recipeTemplateId;
    private bool isDiscover = false;
    public void UpdateVisualRecipeTemplateUI(FinishDishSO finishDishSO)
    {
        //Debug.Log("UpdateVisualRecipeTemplateUI: " + finishDishSO.finishDishName);
        //Debug.Log("recipeTemplateId: " + recipeTemplateId);
        RecipeIcon.gameObject.GetComponent<Image>().sprite = finishDishSO.finishDishIcon;
        RecipeLockIcon.gameObject.GetComponent<Image>().sprite = finishDishSO.finishDishIcon;
    }

    public void SetUnlockRecipe()
    {
        //Debug.Log("This should make the RecipeLockBackground show up");
        RecipeLockBackground.gameObject.SetActive(false);
        isDiscover = true;
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
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

    public bool IsDiscovered()
    {
        return isDiscover;
    }
}
