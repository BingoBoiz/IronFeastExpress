using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableRecipeTemplateUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int index;
    [SerializeField] private Transform OnDragPrefab;
    private Transform todayMenuContent;
    //private Transform recipeDishTransform;
    private RecipeBookTemplateUI recipeBookTemplateUI;
    private Transform OnDraggingPrefab;
    private int dishIndex;
    private FinishDishSO finishDishSOTemp;


    private void Start()
    {
        todayMenuContent = MenuManagerUI.Instance.GetMenuDishContentDraggableTransform();
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag: ");

        recipeBookTemplateUI = RecipeBookUI.Instance.GetRecipeBookTemplateUI(index);

        if (!recipeBookTemplateUI.IsDiscovered())
        {
            return;
        }

        OnDraggingPrefab = Instantiate(OnDragPrefab, transform.root);
        //finishDishSOTemp = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        finishDishSOTemp = RecipeBookManager.Instance.GetDishFromCurrentRecipeBookPageDishList(index - 1);

        OnDraggingPrefab.GetComponent<MenuDishTemplateDraggingGhost>().UpdateMenuDishTemplateVisual(finishDishSOTemp);
        OnDraggingPrefab.SetAsLastSibling();
        OnDraggingPrefab.GetComponent<Image>().raycastTarget = false;

        //recipeDishTransform.gameObject.SetActive(true);

        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!recipeBookTemplateUI.IsDiscovered())
        {
            return;
        }
        // The template clone following the pointer
        OnDraggingPrefab.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!recipeBookTemplateUI.IsDiscovered())
        {
            return;
        }
        // Check if the player drop the template inside todayMenuTransform
        if (IsPointerOverUIObject(todayMenuContent as RectTransform, eventData))
        {
            Debug.Log("Pointer is over the todayMenuTransform.");
            // Are getting the wrong ingredient (NOTE)

            FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByTemplateTransform(recipeBookTemplateUI);
            Debug.Log("The dish from recipe book: " + finishDishSO);


            MenuManagerUI.Instance.AddTemplateToMenuBoardList(finishDishSO);

            //Destroy the clone template
            Destroy(OnDraggingPrefab.gameObject);
            //recipeDishTransform.gameObject.SetActive(false);

        }

        // The player drop the template outside the todayMenuTransform
        else
        {
            Debug.Log("Pointer is not over the parentAfterDrag.");
            Destroy(OnDraggingPrefab.gameObject);
            // Destory in the list (NOTE)
        }
    }

    private bool IsPointerOverUIObject(RectTransform rectTransform, PointerEventData eventData)
    {
        if (rectTransform == null)
            return false;

        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
        return rectTransform.rect.Contains(localMousePosition);
    }
    public void SetDishIndex(int index)
    {
        this.dishIndex = index;
    }
}
