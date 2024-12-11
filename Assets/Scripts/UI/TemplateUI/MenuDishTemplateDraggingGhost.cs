using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuDishTemplateDraggingGhost : MonoBehaviour
{
    [SerializeField] private Image menuDishIcon;
    [SerializeField] private TextMeshProUGUI menuDishText;

    public void UpdateMenuDishTemplateVisual(FinishDishSO menuDishSO)
    {
        menuDishIcon.sprite = menuDishSO.finishDishIcon;
        menuDishText.text = menuDishSO.finishDishName;
    }
}
