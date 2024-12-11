using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuBoardDishTemplateUI : MonoBehaviour
{
    [SerializeField] private Image menuDishIcon;
    [SerializeField] private TextMeshPro menuDishText;
    /*private FinishDishSO dishHold;*/

    public void UpdateMenuDishTemplateVisual(FinishDishSO menuDishSO)
    {
        menuDishIcon.sprite = menuDishSO.finishDishIcon;
        menuDishText.text = menuDishSO.finishDishName;
        /*dishHold = menuDishSO;*/
    }

}
