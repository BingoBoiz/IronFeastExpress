using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private FinishDishListSO recipeListSO;
    private List<FinishDishSO> waitingRecipeSOList;

    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<FinishDishSO>();
    }

    private void Update()
    {
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (waitingRecipeSOList.Count < waitingRecipeMax)
            {
                FinishDishSO waitingRecipeSO = recipeListSO.finishDishSOList[UnityEngine.Random.Range(0, recipeListSO.finishDishSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }

        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++) 
        {
            //Cycling through all the recipe that are waiting for player to finish
            FinishDishSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.ingridientKitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                //Find the waiting recipe has the same number of ingridients with the plate that player just put in
                bool plateContentMatchesRecipe = true;
                foreach(KitchenObjectSO recipeIngredientObjectSO in waitingRecipeSO.ingridientKitchenObjectSOList)
                {
                    //Cycling through all ingredients in the waiting recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        //Cycling through all ingredients in the the plate that player just put in
                        if (plateKitchenObjectSO == recipeIngredientObjectSO)
                        {
                            //Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        // This Recipe ingredient was not found on the plate
                        plateContentMatchesRecipe = false;
                    }
                }
                if (plateContentMatchesRecipe) 
                {
                    //Debug.Log("You just deliver:"+ waitingRecipeSOList[i] + " number: " + i);
                    waitingRecipeSOList.RemoveAt(i);
                    
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }
        
        //Debug.Log("Player did not deliver the right recipe");
    }

    public List<FinishDishSO> GetWaitingRecipeOList()
    {
        return waitingRecipeSOList;
    }
}
