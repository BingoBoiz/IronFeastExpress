using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DraggableRecipeTemplateUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform todayMenuContent;
    private Transform recipeDishTransform;

    private void Start()
    {
        todayMenuContent = MenuManagerUI.Instance.GetTodayMenuContentTransform();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GetComponent<RecipeTemplateUI>().IsDiscovered())
        {
            return;
        }
        //Make a copy of the template to follow on the pointer
        recipeDishTransform = Instantiate(transform, transform.root);
        Debug.Log("OnBeginDrag: " + GetComponent<RecipeTemplateUI>().GetRecipeTemplateId());
        gameObject.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!GetComponent<RecipeTemplateUI>().IsDiscovered())
        {
            return;
        }
        // The template clone following the pointer
        recipeDishTransform.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!GetComponent<RecipeTemplateUI>().IsDiscovered())
        {
            return;
        }
        // Check if the player drop the template inside todayMenuTransform
        if (IsPointerOverUIObject(todayMenuContent as RectTransform, eventData))
        {
            Debug.Log("Pointer is over the todayMenuTransform.");
            // Are getting the wrong ingredient (NOTE)

            FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByTemplateTransform(transform);
            Debug.Log("The dish from recipe book: " + finishDishSO);


            MenuManagerUI.Instance.AddTemplateToDishMenuList(finishDishSO);

            //Destroy the clone template
            Destroy(recipeDishTransform.gameObject);

        }

        // The player drop the template outside the todayMenuTransform
        else
        {
            Debug.Log("Pointer is not over the parentAfterDrag.");
            Destroy(recipeDishTransform.gameObject);
            // Destory in the list (NOTE)
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddTemplateToDishMenuListServerRpc()
    {
        Debug.Log("");

    }

    [ClientRpc]
    private void AddTemplateToDishMenuListClientRpc()
    {

    }

    private bool IsPointerOverUIObject(RectTransform rectTransform, PointerEventData eventData)
    {
        if (rectTransform == null)
            return false;

        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);
        return rectTransform.rect.Contains(localMousePosition);
    }
}
