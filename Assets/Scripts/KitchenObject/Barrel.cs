using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

//using System.Numerics;
using UnityEngine;

public class Barrel : KitchenObject, IContainAmount, ISelectable
{
    /*public event EventHandler OnPlayerGrabbedObject;*/
    public event EventHandler<IContainAmount.OnAmountChangedEventArgs> OnAmountChanged;
    public event EventHandler OnSelectedChanged;

    [SerializeField] private int inventoryTemplateIndex;
    private KitchenObjectSO KitchenObjectSOBarrelHold; 

    private Vector3 heavyObjectLocalScale = new (.6f, .6f, .6f);
    private int kitchenObjectCount;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnAmountChanged: " + kitchenObjectCount);
        OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
        {
            amount = kitchenObjectCount,
        });
    }
    
    public void Interact(Player player)
    {
        //Player already carrying a kitchen object
        if (player.HasKitchenObject())
        {
            Debug.Log("You are carrying something");
        }

        //Player not carrying anything
        else
        {
            Debug.Log("Gain barrel");
            PlayerPickUp(player);
        }

    }

    private void PlayerPickUp(IKitchenObjectParent kitchenObjectParent)
    {
        Debug.Log("PlayerPickUp");

        this.SetItemParent(kitchenObjectParent);
        Debug.Log("SetBarrelParent done");

        NetworkObjectReference networkBarrelReference = GetComponent<NetworkObject>();
        SetBarrelPhysicServerRpc(networkBarrelReference);

        /*if (kitchenObjectParent is MonoBehaviour playerMono)
        {
            Player localPlayer = playerMono.GetComponent<Player>();
            localPlayer.Set
        }*/

        /*this.transform.rotation = UnityEngine.Quaternion.identity;
        this.transform.localScale = heavyObjectLocalScale;
        Collider barrelCollider = this.GetComponent<Collider>();
        Rigidbody barrelRigidbody = this.GetComponent<Rigidbody>();
        if (barrelCollider != null) barrelCollider.enabled = false;
        if (barrelRigidbody != null) barrelRigidbody.isKinematic = true;*/
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBarrelPhysicServerRpc(NetworkObjectReference barrelNetworkObjectReference)
    {
        SetBarrelPhysicClientRpc(barrelNetworkObjectReference);
    }

    [ClientRpc]
    private void SetBarrelPhysicClientRpc(NetworkObjectReference barrelNetworkObjectReference)
    {
        if (barrelNetworkObjectReference.TryGet(out NetworkObject barrelNetworkObject))
        {
            barrelNetworkObject.transform.rotation = UnityEngine.Quaternion.identity;
            barrelNetworkObject.transform.localScale = heavyObjectLocalScale;

            barrelNetworkObject.GetComponent<Collider>().enabled = false;
            barrelNetworkObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public int GetKitchenObjectCount()
    {
        //Debug.Log("Get: " + kitchenObjectCount);
        return kitchenObjectCount;
    }

    public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        KitchenObjectSOBarrelHold = kitchenObjectSO;
    }
    public void SetKitchenObjectCount(int kitchenObjectCount)
    {
        this.kitchenObjectCount = kitchenObjectCount;
        Debug.Log("Set kitchenObjectCount: " + kitchenObjectCount);

        OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
        {
            amount = kitchenObjectCount,
        });
    }

    public KitchenObjectSO GetKitchenObjectSOBarrelHolding()
    {
        return KitchenObjectSOBarrelHold;
    }

}
