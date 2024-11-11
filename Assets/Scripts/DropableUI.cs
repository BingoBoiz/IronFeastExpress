using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropableUI : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        /*GameObject dropped = eventData.pointerDrag;
        DraggableUI draggableUI = dropped.GetComponent<DraggableUI>();
        draggableUI.parentAfterDrag = transform;*/
    }
}
