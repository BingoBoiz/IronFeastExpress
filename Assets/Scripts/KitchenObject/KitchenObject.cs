using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent kitchenObjectParent;
    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        // Clear the counter if it contains an object 
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("IKitchenObjectParent already has a KitchenObject");
            return;
        }
        kitchenObjectParent.SetKitchenObject(this);

        /*Debug.Log(kitchenObjectParent.ToString());*/
        
        //Switch placeHolder to make sure object being spawn at the right counter
        if (followTransform == null)
        {
            Debug.Log("This is null");
        }
        if (kitchenObjectParent == null)
        {
            Debug.Log("This is null");
        }
        if (kitchenObjectParent.GetKitchenObjectFollowTransform() == null)
        {
            Debug.Log("This is null");
        }
        
        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }
    public void SetItemParent(IKitchenObjectParent barrelObjectParent)
    {
        Debug.Log("SetBarrelParent");
        
        SetBarrelParentServerRpc(barrelObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBarrelParentServerRpc(NetworkObjectReference barrelParentNetworkObjectReference)
    {
        Debug.Log("SetBarrelParentServerRpc");
        SetBarrelParentClientRpc(barrelParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetBarrelParentClientRpc(NetworkObjectReference barrelParentNetworkObjectReference)
    {
        Debug.Log("SetBarrelParentClientRpc");

        barrelParentNetworkObjectReference.TryGet(out NetworkObject barrelParentNetworkObject);

        IKitchenObjectParent barrelObjectParent = barrelParentNetworkObject.GetComponent<IKitchenObjectParent>();
        Debug.Log(barrelObjectParent.ToString());
        // Clear the counter if it contains an object 
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = barrelObjectParent;
        if (barrelObjectParent.HasKitchenObject())
        {
            Debug.LogError("IKitchenObjectParent already has a KitchenObject");
        }
        barrelObjectParent.SetKitchenObject(this);

        //Switch placeHolder to make sure object being spawn at the right counter
        followTransform.SetTargetTransform(barrelObjectParent.GetBarrelPlaceHolderTransform());
    }

    public void DestroySelf()
    {
        //kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }

    public void ClearKitchenObjectparent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }
}
