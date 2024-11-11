using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCabinet : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacedHere;

    //[SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private Transform placeHolder;
    [SerializeField] private PlacedInteriorTypeSO placedInteriorTypeSO;

    private KitchenObject kitchenObject;

    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }
    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCabinet.Interact();");
    }
    public virtual void InteractAlternate(Player player)
    {
        Debug.LogError("BaseCabinet.InteractAlternate();");
    }
    public virtual Transform GetKitchenObjectFollowTransform() 
    {
        return placeHolder;
    }
    public virtual void Grab(Player player)
    {
        Debug.LogError("BaseCabinet.Grab();");
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }
    public PlacedInteriorTypeSO GetPlacedInteriorTypeSO()
    {
        return placedInteriorTypeSO;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public Transform GetHeavyPlaceHolderTransform()
    {
        return null;
    }
    public Transform GetBarrelPlaceHolderTransform()
    {
        return null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
