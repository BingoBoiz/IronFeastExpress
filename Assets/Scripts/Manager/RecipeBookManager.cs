using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class RecipeBookManager : NetworkBehaviour
{
    public static RecipeBookManager Instance { get; private set; }

    public static event Action<OnUnlockNewDishAchievementEventArgs> OnUnlockNewDishAchievement;
    public class OnUnlockNewDishAchievementEventArgs : EventArgs
    {
        public FinishDishSO newUnlockFinishDishSO;
    }
    public event EventHandler OnDiscoverNewDish;
    public event EventHandler OnUpdateRecipeBook;

    [SerializeField] private FinishDishListSO finishDishListSO;

    private List<bool> isDishDiscoveryList; // To keep track of what dish is already discover and what dish is not
    private FinishDishSO[] currentRecipeBookPageDishList = new FinishDishSO[4];

    private Transform contentRecipeBookUI;
    private int dishIncreaseDicoveryIndexTesting = 0; // Only for developer to testing by discovery all the dish

    private int recipeTemplateIndex = 0;
    private int pageIndex = 0;

    private void Awake()
    {
        Instance = this;

        if (finishDishListSO.finishDishSOList.Count >= 4)
        {
            for (int i = 0; i < 4; i++)
            {
                currentRecipeBookPageDishList[i] = finishDishListSO.finishDishSOList[i];
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        //PlateKitchenObject.OnUnlockNewDishAchievement += PlateKitchenObject_OnKitchenObjectAchievementEventArgs;
        isDishDiscoveryList = new List<bool>(new bool[finishDishListSO.finishDishSOList.Count]); // The new list with default value is false
        /*for (int i = 0; i < dishDiscoveryList.Count; i++)
        {
            Debug.Log("Check for dishDiscoveryList " + (i+1) + " : " + dishDiscoveryList[i]);
        }*/
    }

    /*private void PlateKitchenObject_OnKitchenObjectAchievementEventArgs(PlateKitchenObject.OnUnlockNewDishAchievementEventArgs obj)
    {
        // When a new dish is being discover, 
        Debug.Log("You just unlock: " + obj.newUnlockFinishDishSO.finishDishName);
        DiscoveryDish(GetFinishDishSOIndex(obj.newUnlockFinishDishSO));
    }*/

    //This should be rename someday, just a terible name
    /*public void SetAllTheRecipeTemplateIntoContent_Old(Transform recipeTemplate, Transform content)
    {
        foreach (Transform child in content)
        {
            Debug.Log("Check if the client is destroyng child");
            Destroy(child.gameObject);
        }

        //Cycle through the FinishDishListSO
        foreach (FinishDishSO finishDishSO in finishDishListSO.finishDishSOList)
        {
            //Debug.Log("Dish Index:" + finishDishListSO.finishDishSOList.IndexOf(finishDishSO));

            // Make a copy of recipeTemplate inside content
            Transform finishDishTransform = Instantiate(recipeTemplate, content);
            finishDishTransform.GetComponent<RecipeTemplateUI_Old>().SetRecipeTemplateID(recipeTemplateIndex);
            recipeTemplateIndex++;
            // Set it active
            int tempRecipeTemplateId = finishDishTransform.GetComponent<RecipeTemplateUI_Old>().GetRecipeTemplateId();
            //Debug.Log("finishDishSO: " + finishDishSO.finishDishName);
            recipeTemplate.GetComponent<RecipeTemplateUI_Old>().Show();

            // Update the visual of the RecipeTemplateUI through an finishDishSO of that copy 'recipeTemplate'
            finishDishTransform.GetComponent<RecipeTemplateUI_Old>().UpdateVisualRecipeTemplateUI(finishDishSO);
            
        }
        // Get transform Content of UI (terible way :>> )
        contentRecipeBookUI = content;
    }*/

    public bool ValidatePageIndex(int pageIndexChangeAmount)
    {
        int pageFutureIndex = pageIndex + pageIndexChangeAmount;
        int totalPages = Mathf.CeilToInt(finishDishListSO.finishDishSOList.Count / 4f) * 2;
        if (pageFutureIndex < 0 || pageFutureIndex >= totalPages)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void ChangeRecipeBookContentByPage(int pageIndexChangeAmount)
    {
        pageIndex += pageIndexChangeAmount;
        
        for (int i = 0; i < 4; i++)
        {
            int dishIndex = pageIndex * 2 + i;

            // Ensure we don't go out of bounds
            if (dishIndex < finishDishListSO.finishDishSOList.Count)
            {
                currentRecipeBookPageDishList[i] = finishDishListSO.finishDishSOList[dishIndex];
            }
            else
            {
                currentRecipeBookPageDishList[i] = null; // You can replace this with a placeholder FinishDishSO if needed
            }
        }
        OnUpdateRecipeBook?.Invoke(this, EventArgs.Empty);
    }
    public List<bool> GetDishDiscoveryList()
    {
        return isDishDiscoveryList;
    }

    public FinishDishSO GetDishFromCurrentRecipeBookPageDishList(int dishNumber)
    {
        return currentRecipeBookPageDishList[dishNumber];
    }

    public FinishDishSO[] GetBookHoldDish()
    {
        return currentRecipeBookPageDishList;
    }

    public void DiscoveryNewDishForTesting(Transform content)
    {
        //FinishDishSO dishSO = GetFinishDishSOByIndex(dishIncreaseDicoveryIndex);
        DiscoveryDish(dishIncreaseDicoveryIndexTesting);
        dishIncreaseDicoveryIndexTesting++;
    }
    public void DiscoveryDish(int dishIndex)
    {
        //int dishIndex = GetFinishDishSOIndex(dishIndex);
        DiscoveryDishServerRpc(dishIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DiscoveryDishServerRpc(int dishIndex)
    {
        DiscoveryDishClientRpc(dishIndex);
    }

    [ClientRpc]
    private void DiscoveryDishClientRpc(int dishIndex)
    {
        Debug.Log("dishIndex: " + dishIndex);
        FinishDishSO unlockDish = GetFinishDishSOByIndex(dishIndex);
        isDishDiscoveryList[dishIndex] = true;
        
        OnUnlockNewDishAchievement?.Invoke(new OnUnlockNewDishAchievementEventArgs
        {
            newUnlockFinishDishSO = unlockDish,
        });
        AchievementManager.Instance.RemoveLockedDishList(unlockDish);

        OnDiscoverNewDish?.Invoke(this, EventArgs.Empty);
    }

    public FinishDishSO GetFinishDishSOByTemplateTransform(RecipeBookTemplateUI recipeBookTemplateUI)
    {
        int recipeTemplateIndexGet = recipeBookTemplateUI.GetRecipeTemplateId();
        Debug.Log("check GetFinishDishSOByTemplateTransform(): " + recipeTemplateIndexGet);
        return finishDishListSO.finishDishSOList[recipeTemplateIndexGet];
    }
    
    public FinishDishSO GetFinishDishSOByIndex(int finishDishSOIndex)
    {
        try
        {
            if (finishDishSOIndex >= 0 && finishDishSOIndex < finishDishListSO.finishDishSOList.Count)
            {
                return finishDishListSO.finishDishSOList[finishDishSOIndex];
            }
            else
            {
                Debug.LogError($"Index {finishDishSOIndex} is out of bounds. List size is {finishDishListSO.finishDishSOList.Count}.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in GetFinishDishSOByIndex: {e.Message}");
            return null;
        }
    }

    public int GetFinishDishSOIndex(FinishDishSO finishDishSO)
    {
        try
        {
            // Check if the finishDishSO is valid and the list is not null
            if (finishDishSO == null)
            {
                Debug.LogError("The provided finishDishSO is null.");
                return -1;
            }

            if (finishDishListSO.finishDishSOList != null)
            {
                int index = finishDishListSO.finishDishSOList.IndexOf(finishDishSO);
                if (index >= 0)
                {
                    return index;
                }
                else
                {
                    Debug.LogError("FinishDishSO not found in the list.");
                    return -1;
                }
            }
            else
            {
                Debug.LogError("The FinishDishSO list is null.");
                return -1;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in GetFinishDishSOIndex: {e.Message}");
            return -1;
        }
    }

    public bool IsDiscoveredDish(FinishDishSO dishSO)
    {
        return isDishDiscoveryList[finishDishListSO.finishDishSOList.IndexOf(dishSO)];
    }

    public int GetCurrentPage()
    {
        return pageIndex + 1;
    }

    public int GetBookMaxPage()
    {
        if (finishDishListSO == null || finishDishListSO.finishDishSOList == null || finishDishListSO.finishDishSOList.Count == 0)
        {
            return 0; // No dishes, no pages
        }

        // Each page contains 2 dishes, so calculate the total pages needed
        return Mathf.CeilToInt(finishDishListSO.finishDishSOList.Count / 4f) * 2;
    }
}
