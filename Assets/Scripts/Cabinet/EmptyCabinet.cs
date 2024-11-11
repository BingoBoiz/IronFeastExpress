using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCabinet : BaseCabinet
{
    //[SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        
        if (!HasKitchenObject())
        {
            //Debug.Log("The counter is empty");
            if (player.HasKitchenObject())
            {
                //Debug.Log("Player is carrying something");
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }

            else
            {
                //Debug.Log("Player not carrying anything");
                Debug.Log("There is nothing to interact");
            }
        }
        
        else
        {
            //Debug.Log("There is a kitchen object on the counter");
            if (player.HasKitchenObject()) 
            {
                //Debug.Log("The player is carrying something");
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //Debug.Log("The player is holding a plate");
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        //GetKitchenObject().DestroySelf();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }

                else
                {
                    //Debug.Log("The player is not holding a plate");
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                        }
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

    public override void InteractAlternate(Player player)
    {
        if (player.HasKitchenObject()) // Player maybe is holding a crate
        {
            Debug.Log("Player is holding a kitchenObject");
        }
        else if (player.gameObject.GetComponent<PlayerPlacingCabinet>().IsHoldingCabinet())
        {
            Debug.Log("Player is holding a cabinet");
        }
        else if (GameManager.Instance.IsTrainStop())
        {
            PlayerPlacingCabinet.LocalInstance.PickUpInterior(GetComponent<PlacedCabinet_Done>());
        }
        else
        {
            Debug.LogWarning("InteractAlternate nothing");
        }
    }

}
