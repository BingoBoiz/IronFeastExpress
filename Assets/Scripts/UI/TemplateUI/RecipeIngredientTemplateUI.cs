using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeIngredientTemplateUI : MonoBehaviour
{
    private const int MAXIMUM_INGREDIENT_IN_RECIPE = 3;

    [SerializeField] private Image ingredientDishIcon1;
    [SerializeField] private Image ingredientDishIcon2;
    [SerializeField] private Image ingredientDishIcon3;
    [SerializeField] private Sprite ingredientDishIconNULL;

    public void UpdateRecipeIngredientTemplateVisual(FinishDishSO menuDishSO)
    {
        int ingredientIndex = 0;
        foreach (KitchenObjectSO ingredientKitchenObjectSO in menuDishSO.ingridientKitchenObjectSOList)
        {
            //Debug.Log(menuDishSO.finishDishName + " cycle through " + ingredientKitchenObjectSO.objectName + " match with " + ingredientIndex +" index.");
            switch (ingredientIndex)
            {
                case 0:
                    ingredientDishIcon1.sprite = ingredientKitchenObjectSO.sprite;
                    break;
                case 1:
                    ingredientDishIcon2.sprite = ingredientKitchenObjectSO.sprite;
                    break;
                case 2:
                    ingredientDishIcon3.sprite = ingredientKitchenObjectSO.sprite;
                    break;
                default:
                    Debug.LogError("ingredientIndex :" + ingredientIndex);
                    break;
            }
            ingredientIndex++;
        }
        if (ingredientIndex < MAXIMUM_INGREDIENT_IN_RECIPE) // There is less than 3 ingredient in this recipe
        {
            for (int i = ingredientIndex; i < MAXIMUM_INGREDIENT_IN_RECIPE; i++)
            {
                switch (i)
                {
                    case 1:
                        ingredientDishIcon2.sprite = ingredientDishIconNULL;
                        break;
                    case 2:
                        ingredientDishIcon3.sprite = ingredientDishIconNULL;
                        break;
                    default:
                        Debug.LogError("Invalid ingredient index: " + i);
                        break;
                }
            }
        }
    }
}
