using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;


public class ContainerCabinet : BaseCabinet, IContainAmount
{
    //[SerializeField] private int inventoryTemplateIndex;

    public event EventHandler OnPlayerGrabbedObject;
    public event EventHandler<IContainAmount.OnAmountChangedEventArgs> OnAmountChanged;

    private int containerCabinetHoldAmount;
    private KitchenObjectSO kitchenObjectSO;
    private Barrel barrelInteracted;


    private void Start()
    {
        containerCabinetHoldAmount = 0;
        kitchenObjectSO = null;
        barrelInteracted = null;
    }

    public override void Interact(Player player)
    {
        //Player already carrying a kitchen object
        if (player.HasKitchenObject())
        {
            Debug.Log("You are carrying something");
            if (player.GetKitchenObject().TryGetComponent<Barrel>(out Barrel barrel))
            {
                containerCabinetHoldAmount += barrel.GetKitchenObjectCount();
                
                Debug.Log("containerCabinetHoldAmount: " + containerCabinetHoldAmount);
                NetworkObjectReference barrelNetworkRef = barrel.GetComponent<NetworkObject>();
                PlayerPutKitchenObjectServerRpc(containerCabinetHoldAmount, barrelNetworkRef);

                KitchenObject.DestroyKitchenObject(barrel);
            }
        }

        else if (containerCabinetHoldAmount < 1)
        {
            return;
        }

        //Player not carrying anything
        else
        {
            Debug.Log("Container cabinet loose kitchen object!");
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);
            PlayerTakeKitchenObjectServerRpc();
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

    [ServerRpc(RequireOwnership = false)]
    private void PlayerTakeKitchenObjectServerRpc()
    {
        PlayerTakeKitchenObjectClientRpc();
    }

    [ClientRpc]
    private void PlayerTakeKitchenObjectClientRpc() 
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);

        containerCabinetHoldAmount--;

        // This is for NumberShownUI.cs
        OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
        {
            amount = containerCabinetHoldAmount,
        });

        // If run out of kitchen object
        if (containerCabinetHoldAmount == 0)
        {
            kitchenObjectSO = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerPutKitchenObjectServerRpc(int containerCabinetHoldAmount, NetworkObjectReference kitchenObjectNetworkReference)
    {
        Debug.Log("PlayerPutKitchenObjectServerRpc");
        PlayerPutKitchenObjectClientRpc(containerCabinetHoldAmount, kitchenObjectNetworkReference);
    }

    [ClientRpc]
    private void PlayerPutKitchenObjectClientRpc(int containerCabinetHoldAmount, NetworkObjectReference kitchenObjectNetworkReference)
    {
        Debug.Log("PlayerPutKitchenObjectClientRpc");
        if(kitchenObjectNetworkReference.TryGet(out NetworkObject kitchenObjectNetworkObject))
        {
            Barrel barrel = kitchenObjectNetworkObject.GetComponent<Barrel>();
            kitchenObjectSO = barrel.GetKitchenObjectSOBarrelHolding();
            //Debug.Log("KitchenObjectSO: " + barrel.GetKitchenObjetcSO());
            this.containerCabinetHoldAmount = containerCabinetHoldAmount;
            OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
            {
                amount = containerCabinetHoldAmount,
            });
        }
        
    }

}
