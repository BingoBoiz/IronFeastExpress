using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenBuild : MonoBehaviour
{
    /*private List<StoreProductSO> GetDailyRandomStoreProductSOList() { return null; }
    private void StoreProductSingleUI_OnStoreProductCountChanged1(object sender, StoreProductTemplateUI.OnStoreProductCountChangedEventArgs e)
    {
        bool AddNewProduct = false;
        // Find the matching product in the dailyRandomStoreProductSOList by name
        foreach (StoreProductSO storeProductSO in GetDailyRandomStoreProductSOList())
        {
            // If it already on the cart list
            if (cartOrderStoreProductSOList.Contains(storeProductSO) && storeProductSO.kitchenObjectSO.objectName == e.productName)
            {
                int index = cartOrderStoreProductSOList.FindIndex(x => x == storeProductSO);
                cartOrderStoreProductCountList[index]++;
                CalculateTotalCost(cartOrderStoreProductSOList[index].productPrice * e.changeAmount);
                return;
            }

            else   // If not exist in the cart list
            {
                if (storeProductSO.kitchenObjectSO.objectName == e.productName)
                {
                    buyStoreProductSO = storeProductSO;
                    AddNewProduct = true;
                    break;
                }
            }
        }

        if (AddNewProduct)
        {
            Debug.Log("The price of the " + buyStoreProductSO.kitchenObjectSO.objectName + " that you just add is: " + buyStoreProductSO.productPrice);
            cartOrderStoreProductSOList.Add(buyStoreProductSO);
            cartOrderStoreProductCountList.Add(e.changeAmount);
            Debug.Log("Count list: " + cartOrderStoreProductSOList.Count);
            CalculateTotalCost(buyStoreProductSO.productPrice * e.changeAmount);

            //Reset the loop
            AddNewProduct = false;
            buyStoreProductSO = null;
        }

        else
        {
            Debug.LogWarning("Cannot found the product name");
        }
    }*/

    /*public void SetAllTheStoreProductTemplateIntoContent(Transform storeProductTemplate, Transform content)
    {
        // Clear content before adding new items (optional but recommended)
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        //Cycle through the Random List
        foreach (StoreProductSO storeProductSO in dailyRandomStoreProductSOList)
        {
            // Make a copy of storeProductSingleTemplate inside content
            Transform productStoreTransform = Instantiate(storeProductTemplate, content);
            productStoreTransform.gameObject.SetActive(true);

            // Update the visual of the StoreProductSingleUI through an StoreProductSO of that copy 'storeProductSingleTemplate'
            var storeProductTemplateUI = productStoreTransform.GetComponent<StoreProductTemplateUI>();
            storeProductTemplateUI.UpdateVisualStoreProductUI(storeProductSO);
            storeProductTemplateUI.OnStoreProductCountChanged += StoreProductSingleUI_OnStoreProductCountChanged;

        }
        // Store the content reference for future use
        contentStoreManagerUI = content;
    }

    private void StoreProductSingleUI_OnStoreProductCountChanged(object sender, StoreProductTemplateUI.OnStoreProductCountChangedEventArgs e)
    {
        // Find the matching product in the dailyRandomStoreProductSOList by name
        StoreProductSO matchingProduct = dailyRandomStoreProductSOList.FirstOrDefault(product => product.kitchenObjectSO.objectName == e.productName);

        if (matchingProduct == null)
        {
            Debug.LogWarning("Cannot find the product name: " + e.productName);
            return;
        }

        // Check if the product is already in the cart
        int productIndex = cartOrderStoreProductSOList.IndexOf(matchingProduct);
        if (productIndex >= 0)
        {
            // Product already in the cart, update its count and recalculate total cost
            cartOrderStoreProductCountList[productIndex] += e.changeAmount;
            CalculateTotalCost(matchingProduct.productPrice * e.changeAmount);
        }
        else
        {
            // Add new product to the cart
            cartOrderStoreProductSOList.Add(matchingProduct);
            cartOrderStoreProductCountList.Add(e.changeAmount);
            CalculateTotalCost(matchingProduct.productPrice * e.changeAmount);
        }

        Debug.Log("The price of the " + matchingProduct.kitchenObjectSO.objectName + " is: " + matchingProduct.productPrice);
    }*/
}
