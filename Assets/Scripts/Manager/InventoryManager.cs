using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }
    public event EventHandler<OnBuyProductEventArgs> OnBuyProduct;

    public class OnBuyProductEventArgs : EventArgs // Firing event for Container Cabinet 
    {
        public int inventorySlotHoldAmountInt;
        public KitchenObjectSO inventorySlotTemplateKitchenObjectSO;
    }

    private float money = 100;
    private int inventorySlotCurrentHave = 3;
    private Transform contentInventoryManagerUI;
    private List<KitchenObjectSO> inventorySlotHoldKitchenObjectList; //represent the inventory slot
    private List<int> countKichenObjectInventorySlotList; //represent the items count of that inventory slot

    //private int inventorySlotMax = 4;

    private void Awake()
    {
        Instance = this;
        inventorySlotHoldKitchenObjectList = new List<KitchenObjectSO>();
        countKichenObjectInventorySlotList = new List<int>();
        for (int i = 0; i < inventorySlotCurrentHave; i++) 
        {
            //Fill 'null' to every KitchenObjectSO in the List represent the empty inventory slot 
            inventorySlotHoldKitchenObjectList.Add(null);
            countKichenObjectInventorySlotList.Add(0);
        }
    }

    public void BuyProductToInventory(float productPrice, KitchenObjectSO addKitchenObjectSO)
    {
        bool findEmptySlot = false;
        int emptySlotIndex = -1;
        bool findTheSameKitchenObjectInInventory = false;
        int sameKitchenObjectSlotIndex = -1;

        // Not enough money
        if (money < productPrice)
        {
            Debug.Log("You don't have enough money");
            return;
        }

        //Cycle through inventory slot list to find the slot that have the same KitchenObject or empty slot 
        for (int i = 0; i < inventorySlotCurrentHave; i++) 
        {
            if (addKitchenObjectSO == inventorySlotHoldKitchenObjectList[i])
            {
                findTheSameKitchenObjectInInventory = true;
                sameKitchenObjectSlotIndex = i;
                break;
            }

            if (inventorySlotHoldKitchenObjectList[i] == null)
            {
                //Use the first empty slot by checking findEmptySlot
                if (!findEmptySlot)
                {
                    emptySlotIndex = i;
                }

                findEmptySlot = true;
            }
           
        }

        // Prioritize the slot that have the same KitchenObject
        if (findTheSameKitchenObjectInInventory)
        {
            if (sameKitchenObjectSlotIndex == -1)
            {
                Debug.LogError("This code should never reach!!!");
                Debug.LogError("The slot index: " + sameKitchenObjectSlotIndex);
            }
            Debug.Log(addKitchenObjectSO + "just increase 1");
            countKichenObjectInventorySlotList[sameKitchenObjectSlotIndex]++;

            OnBuyProduct.Invoke(this, new OnBuyProductEventArgs
            {
                inventorySlotHoldAmountInt = countKichenObjectInventorySlotList[sameKitchenObjectSlotIndex],
                inventorySlotTemplateKitchenObjectSO = inventorySlotHoldKitchenObjectList[sameKitchenObjectSlotIndex]

            });

            // Update the visual of the slot template
            InventorySlotTemplateUI templateHoldNewKitchenObjectTransform = contentInventoryManagerUI.GetChild(sameKitchenObjectSlotIndex).GetComponent<InventorySlotTemplateUI>();
            templateHoldNewKitchenObjectTransform.UpdateVisualInventoryTemplateUI(inventorySlotHoldKitchenObjectList[sameKitchenObjectSlotIndex], countKichenObjectInventorySlotList[sameKitchenObjectSlotIndex]);

            // Update the money
            money -= productPrice;

        }

        // If cannot find the same KitchenObject but still have an empty slot
        else if (findEmptySlot)
        {
            if (emptySlotIndex == -1)
            {
                Debug.LogError("This code should never reach!!!");
                Debug.LogError("The empty slot index: " + emptySlotIndex);
            }
            //Find the empty slot to put KitchenObject inside
            inventorySlotHoldKitchenObjectList[emptySlotIndex] = addKitchenObjectSO;
            countKichenObjectInventorySlotList[emptySlotIndex]++; //Usually will equal to 1

            
            OnBuyProduct.Invoke(this, new OnBuyProductEventArgs
            {
                inventorySlotHoldAmountInt = countKichenObjectInventorySlotList[emptySlotIndex],
                inventorySlotTemplateKitchenObjectSO = inventorySlotHoldKitchenObjectList[emptySlotIndex]
            });

            //Update the visual of the slot template
            InventorySlotTemplateUI templateHoldNewKitchenObjectTransform = contentInventoryManagerUI.GetChild(emptySlotIndex).GetComponent<InventorySlotTemplateUI>();
            templateHoldNewKitchenObjectTransform.UpdateVisualInventoryTemplateUI(inventorySlotHoldKitchenObjectList[emptySlotIndex], countKichenObjectInventorySlotList[emptySlotIndex]);

            // Update the money
            money -= productPrice;
        }

        else
        {
            Debug.Log("Inventory is full!!!");
        }
    }

    public void SetAllTheInventoryBoxTemplateIntoContent(Transform inventoryBoxTemplate, Transform content)
    {
        //Cycle through the content to destroy everything non-relevant
        foreach (Transform child in content)
        {
            if (child == inventoryBoxTemplate)
            {
                //Debug.Log("inventoryBoxTemplate get ignore");
                continue;
            }
            Destroy(child.gameObject);
            //Debug.Log("A gameObject just get destroy");
        }

        //Cycle through all inventory slot (reason why i start with 1 because there is already 1 template in the editor)
        for (int i = 1; i < inventorySlotCurrentHave; i++)
        {
            // Make a copy of inventoryBoxTemplate inside content
            Transform inventoryBoxTemplateTransform = Instantiate(inventoryBoxTemplate, content);

            // Set it active
            inventoryBoxTemplate.gameObject.SetActive(true);
            
            if (inventorySlotHoldKitchenObjectList[i] == null)
            {
                //Debug.Log("The inventory slot number " + i + " is empty !");
            }
            else
            {
                Debug.Log("The inventory slot number " + i + " hold " + inventorySlotHoldKitchenObjectList[i].objectName);

                // Update the visual of the InventorySlotTemplateUI through an KitchenObjectSO of that copy 'inventoryBoxTemplate'
                inventoryBoxTemplate.GetComponent<InventorySlotTemplateUI>().UpdateVisualInventoryTemplateUI(inventorySlotHoldKitchenObjectList[i], countKichenObjectInventorySlotList[i]);
            }
            // Get transform Content of UI (terible way :>> )
            contentInventoryManagerUI = content;
        }
    }

    public KitchenObjectSO GetInventorySlotByIndex(int index)
    {
        return inventorySlotHoldKitchenObjectList[index];
    }

    public int GetObjectAmountInventorySlotByIndex(int index)
    {
        return countKichenObjectInventorySlotList[index];
    }

    public void DecreaseCountKitchenObjects(int index, int lostCount)
    {
        Debug.Log("Index: " + index);
        countKichenObjectInventorySlotList[index] -= lostCount;
        //InventorySlotTemplateUI inventorySlotDecrease = contentInventoryManagerUI.GetChild(index).GetComponent<InventorySlotTemplateUI>();

        if (inventorySlotHoldKitchenObjectList[index] == null)
        {
            Debug.LogError("inventorySlotHoldKitchenObjectList is null!!!");
        }

        //inventorySlotDecrease.UpdateVisualInventoryTemplateUI(inventorySlotHoldKitchenObjectList[index], countKichenObjectInventorySlotList[index]);

        if (countKichenObjectInventorySlotList[index] == 0)
        {
            //inventorySlotDecrease.RemoveKitchenObjectFromInventoryTemplateUI();
            inventorySlotHoldKitchenObjectList[index] = null;
        }
    }

    public float GetMoney()
    {
        return money;
    }

    
}
