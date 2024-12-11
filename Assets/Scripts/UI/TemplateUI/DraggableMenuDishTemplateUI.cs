using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableMenuDishTemplateUI : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image image;
    [HideInInspector] public Transform parentAfterDrag;

    [SerializeField] private Transform OnDragPrefab;
    private int dishIndex;
    private Transform OnDraggingPrefab;
    private FinishDishSO finishDishSOTemp;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        /*gameObject.SetActive(false);*/
        OnDraggingPrefab = Instantiate(OnDragPrefab, transform.root);
        finishDishSOTemp = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        OnDraggingPrefab.GetComponent<MenuDishTemplateDraggingGhost>().UpdateMenuDishTemplateVisual(finishDishSOTemp);
        OnDraggingPrefab.SetAsLastSibling();
        OnDraggingPrefab.GetComponent<Image>().raycastTarget = false;

        /*OnDragPrefab.SetParent(transform.root); // transform.root is The Canvas
        transform.SetAsLastSibling(); //Move it to top so it cannot be cover by anything

        image.raycastTarget = false; // make sure the image doesnt block the raycast for indetify dropable places*/
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Input.mousePosition: " + Input.mousePosition);
        OnDraggingPrefab.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Check if the tranform is parentAfterDrag
        if (IsPointerOverUIObject(parentAfterDrag as RectTransform, eventData)) //NOTE: This should destroy new transform and make a copy of menuDishTemplate + updatevisual that template
        {
            Debug.Log("Pointer is over the parentAfterDrag. Notthing happen");
            Destroy(OnDraggingPrefab.gameObject);
            /*transform.SetParent(parentAfterDrag);
            image.raycastTarget = false;*/
        }
        // If the tranform is not parentAfterDrag
        else
        {
            Debug.Log("Pointer is not over the parentAfterDrag.");

            Debug.Log("Proceed to destroy this object.");
            Destroy(OnDraggingPrefab.gameObject);
            MenuManagerUI.Instance.RemoveTemplateToMenuBoardList(dishIndex);
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
