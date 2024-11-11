using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class CuttingCabinet : BaseCabinet, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    public event EventHandler OnCutObject;
    


    [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;
    private int cuttingProgress;


    public override void Interact(Player player)
    {
        //Debug.Log("Check if the counter is empty");
        if (!HasKitchenObject())
        {
            //Debug.Log("Check if the player carrying something");
            if (player.HasKitchenObject())
            {
                Debug.Log("Check if the object cuttable");
                KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
                Debug.Log("playerKitchenObjectSO: " + playerKitchenObjectSO);
                if (HasRecipeWithInput(playerKitchenObjectSO))
                {
                    Debug.Log("Process to cut the object");
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCabinetServerRpc();
                }
                else 
                {
                    Debug.Log("Can not cut this object");
                }
                
            }
            // Player not carrying anything
            else
            {
                //Debug.Log("There is nothing to interact");
            }
        }
        // There is a kitchen object on the counter
        else
        {
            // Check if the player carrying something
            if (player.HasKitchenObject())
            {
                //Debug.Log("There is already a kitchen object in this counter");
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
            // Player not carrying anything
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCabinetServerRpc()
    {
        InteractLogicPlaceObjectOnCabinetClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCabinetClientRpc()
    {
        cuttingProgress = 0;
        //CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSWithInput(kitchenObject.GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            //progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate(Player player)
    {
        //There is a kitchen object here
        if (HasKitchenObject()) 
        {
            if (HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()) && GameManager.Instance.IsTrainRunning())
            {
                CutObjectServerRpc();
                CheckCuttingProgressDoneServerRpc();
            }
        }
        else if (player.HasKitchenObject()) // Player maybe is holding a crate
        {
            Debug.Log("Player is holding a kitchenObject");
        }
        else if (player.GetComponent<PlayerPlacingCabinet>().IsHoldingCabinet())
        {
            Debug.Log("Player is holding a cabinet");
        }
        else if (GameManager.Instance.IsTrainStop())
        {
            PlayerPlacingCabinet.LocalInstance.PickUpInterior(GetComponent<PlacedCabinet_Done>());
        }
        else
        {
            Debug.Log("CuttingCabinet InteractAlternate");
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        Debug.Log("CutObjectServerRpc");
        CutObjectClientRpc();

    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress++;
        OnCutObject?.Invoke(this, EventArgs.Empty);
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckCuttingProgressDoneServerRpc()
    {
        Debug.Log("CheckCuttingProgressDoneServerRpc");
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSWithInput(GetKitchenObject().GetKitchenObjectSO());

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                KitchenObjectSO inputKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                KitchenObjectSO outputKitchenObjectSO = GetRecipeOutput(inputKitchenObjectSO);
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    private KitchenObjectSO GetRecipeOutput(KitchenObjectSO inputKitchenObjectSO)
    {
        
        
        foreach (CuttingRecipeSO kitchenRecipeSO in cutKitchenObjectSOArray)
        {
            if (kitchenRecipeSO.input == inputKitchenObjectSO)
            {
                return kitchenRecipeSO.output;
            }
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO kitchenRecipeSO in cutKitchenObjectSOArray)
        {
            if (kitchenRecipeSO.input == inputKitchenObjectSO)
            {
                return true;
            }
        }
        return false;
    }

    private CuttingRecipeSO GetCuttingRecipeSWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO kitchenRecipeSO in cutKitchenObjectSOArray)
        {
            if (kitchenRecipeSO.input == inputKitchenObjectSO)
            {
                return kitchenRecipeSO;
            }
        }
        return null;
    }
}
