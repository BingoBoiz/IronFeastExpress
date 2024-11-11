using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlotTemplateUI : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI slotKitchenObjectCount;
    [SerializeField] private TextMeshProUGUI kitchenObjectName;
    [SerializeField] private Transform kitchenObjectIcon;

    private Sprite defaultEmptyIcon;
    private int inventorySlotIndex;

    private void Start()
    {
        defaultEmptyIcon = kitchenObjectIcon.GetComponent<Image>().sprite;
    }

    public void UpdateVisualInventoryTemplateUI(KitchenObjectSO kitchenObjectSO, int slotCountKitchenObject)
    {
        if (kitchenObjectSO == null) 
        {
            Debug.LogError("kitchenObjectSO is null!!");
        }
        if (slotCountKitchenObject < 0)
        {
            Debug.LogError("slotCountKitchenObject is null");
        }
        slotKitchenObjectCount.text = slotCountKitchenObject.ToString();
        kitchenObjectName.text = kitchenObjectSO.objectName;
        kitchenObjectIcon.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
    }

    public void RemoveKitchenObjectFromInventoryTemplateUI()
    {
        kitchenObjectName.text = "Empty";
        kitchenObjectIcon.GetComponent<Image>().sprite = defaultEmptyIcon;
    }
}
