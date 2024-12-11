using System;
using Unity.Netcode;
using UnityEngine;

public class Crate : KitchenObject, IContainAmount, ISelectable
{
    /*public event EventHandler OnPlayerGrabbedObject;*/

    public event EventHandler<IContainAmount.OnAmountChangedEventArgs> OnAmountChanged;
    public event EventHandler OnSelectedChanged;

    private PlacedInteriorTypeSO placedCabinetTypeCrateHold; 

    private Vector3 heavyObjectLocalScale = new(.31f, .31f, .31f);
    private int cabinetCount;

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnAmountChanged: " + cabinetCount);
        OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
        {
            amount = cabinetCount,
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
            Debug.Log("Gain crate");
            PlayerPickUp(player);

            /*OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);*/
        }
    }
    private void PlayerPickUp(IKitchenObjectParent kitchenObjectParent)
    {
        Debug.Log("PlayerPickUp");
        SetItemParent(kitchenObjectParent);
        NetworkObjectReference networkCrateReference = GetComponent<NetworkObject>();
        SetCratePhysicServerRpc(networkCrateReference);

        if (kitchenObjectParent is MonoBehaviour playerMono)
        {
            PlayerPlacingCabinet localPlayer = playerMono.GetComponent<PlayerPlacingCabinet>();
            int placedCabinetTypeSOIndex = TrainManager.Instance.GetPlacedCabinetTypeSOIndex(placedCabinetTypeCrateHold);
            Debug.Log(placedCabinetTypeSOIndex);
            localPlayer.RecieveCabinet(placedCabinetTypeSOIndex, cabinetCount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCratePhysicServerRpc(NetworkObjectReference crateNetworkObjectReference)
    {
        SetCratePhysicClientRpc(crateNetworkObjectReference);
    }

    [ClientRpc]
    private void SetCratePhysicClientRpc(NetworkObjectReference crateNetworkObjectReference)
    {
        if (crateNetworkObjectReference.TryGet(out NetworkObject crateNetworkObject))
        {
            crateNetworkObject.transform.rotation = UnityEngine.Quaternion.identity;
            crateNetworkObject.transform.localScale = heavyObjectLocalScale;

            crateNetworkObject.GetComponent<Collider>().enabled = false;
            crateNetworkObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void SetPlacedCabinetTypeSO(PlacedInteriorTypeSO placedCabinetTypeSO)
    {
        placedCabinetTypeCrateHold = placedCabinetTypeSO;
    }
    public void SetCabinetCount(int cabinetCount)
    {
        this.cabinetCount = cabinetCount;
        Debug.Log("Set kitchenObjectCount: " + cabinetCount);

        OnAmountChanged?.Invoke(this, new IContainAmount.OnAmountChangedEventArgs
        {
            amount = cabinetCount,
        });
    }
}
