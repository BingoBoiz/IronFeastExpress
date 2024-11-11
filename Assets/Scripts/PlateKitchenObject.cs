using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    [SerializeField] private FinishDishListSO validFinishDishListSO;

    private List<KitchenObjectSO> kitchenObjectSOContainList;
    private List<FinishDishSO> lockedDishList;

    //private FinishDishSO unlockDishRpc;


    public override void OnNetworkSpawn()
    {
        kitchenObjectSOContainList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            // Not a valid ingredient 
            return false;
        }

        else if (kitchenObjectSOContainList.Contains(kitchenObjectSO)) 
        {
            // The dish already contains that kitchenObjectSO
            return false;
        }

        else
        {
            int kitchenObjectSOIndex = KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO);
            AddIngredientServerRpc(kitchenObjectSOIndex);
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
        if (IsNewDish(out FinishDishSO unlockDishRpc))
        {
            int unlockDishIndex = RecipeBookManager.Instance.GetFinishDishSOIndex(unlockDishRpc);
            //DiscoverNewDishClientRpc(unlockDishIndex);
            RecipeBookManager.Instance.DiscoveryDish(unlockDishIndex);
        }
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex) 
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOByIndex(kitchenObjectSOIndex);
        kitchenObjectSOContainList.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            kitchenObjectSO = kitchenObjectSO,
        });
    }

    /*[ClientRpc]
    private void DiscoverNewDishClientRpc(int unlockDishIndex)
    {
        FinishDishSO unlockDish = RecipeBookManager.Instance.GetFinishDishSOByIndex(unlockDishIndex);
        OnUnlockNewDishAchievement?.Invoke(new OnUnlockNewDishAchievementEventArgs
        {
            newUnlockFinishDishSO = unlockDish,
        });
        AchievementManager.Instance.RemoveLockedDishList(unlockDish);
    }*/

    private bool IsNewDish(out FinishDishSO unlockDishRpc)
    {
        List<bool> listDiscoveryDish = RecipeBookManager.Instance.GetDishDiscoveryList();
        lockedDishList = new List<FinishDishSO>(AchievementManager.Instance.GetLockedDishList());

        foreach (FinishDishSO dish in lockedDishList)
        {
            //Debug.Log("Dish" + dish.finishDishName + " have ingredient count: " + dish.ingridientKitchenObjectSOList.Count);
            if (dish.ingridientKitchenObjectSOList.Count == kitchenObjectSOContainList.Count && dish.ingridientKitchenObjectSOList.All(kitchenObjectSOContainList.Contains))
            {
                Debug.Log("Dish unlock: " + dish);
                int dishIndex = RecipeBookManager.Instance.GetFinishDishSOIndex(dish);
                unlockDishRpc = dish;
                return true;
            }
        }
        unlockDishRpc = null;
        return false;
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOContainList;
    }
}
