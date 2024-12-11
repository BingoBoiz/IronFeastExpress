using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDishTemplateUI_Old : MonoBehaviour
{
    private const int MAXIMUM_INGREDIENT_IN_RECIPE = 3;
    [SerializeField] private Image finishDishIcon;
    [SerializeField] private Image ingredientDishIcon1;
    [SerializeField] private Image ingredientDishIcon2;
    [SerializeField] private Image ingredientDishIcon3;

    //[SerializeField] private List<Image> ingredientIconList;
    private void Awake()
    {
        //ingredientIconList = new List<Image>(MAXIMUM_INGREDIENT_IN_RECIPE);

        /*for (int i = 0; i < MAXIMUM_INGREDIENT_IN_RECIPE; i++)
        {
            ingredientIconList.Add(finishDishIcon);
            
        }*/
    }

    public void UpdateMenuDishTemplateVisual(FinishDishSO menuDishSO)
    {

        finishDishIcon.sprite = menuDishSO.finishDishIcon;

        //Debug.Log("Update sprite: "+finishDishIcon.sprite.ToString());
        int ingredientIndex = 0;


        foreach (KitchenObjectSO ingredientKitchenObjectSO in menuDishSO.ingridientKitchenObjectSOList)
        {
            //Debug.Log(menuDishSO.finishDishName + " cycle through " + ingredientKitchenObjectSO.objectName + " match with " + ingredientIndex +" index.");
            switch (ingredientIndex) {
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
            if(MAXIMUM_INGREDIENT_IN_RECIPE - ingredientIndex == 1)
            {
                Debug.Log(menuDishSO.finishDishName + " have empty ingredient in " + ingredientIndex + " index.");
                ingredientDishIcon3.sprite = null;
            }
        }

        /*for (int i = 0; i < MAXIMUM_INGREDIENT_IN_RECIPE; i++)
        {
            Debug.Log("i: "+i);
            ingredientIconList[i].sprite = menuDishSO.ingridientKitchenObjectSOList[i].sprite;
        }*/
    }

}
