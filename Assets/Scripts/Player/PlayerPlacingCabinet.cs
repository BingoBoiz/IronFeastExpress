using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//using static PlacedCabinetTypeSO;
//using static TrainFloorGrid;

public class PlayerPlacingCabinet : NetworkBehaviour
{
    public static PlayerPlacingCabinet LocalInstance { get; private set; }

    public event EventHandler<OnSelectedCabinetChangedEventArgs> OnSelectedCabinetChanged;
    public class OnSelectedCabinetChangedEventArgs : EventArgs
    {
        public int placedCabinetTypeSOIndex;
    }
    public event EventHandler<OnPlacingCabinetEventArgs> OnPlacingCabinet;
    public class OnPlacingCabinetEventArgs : EventArgs
    {
        public int placedCabinetTypeSOIndex;
    }
    public event EventHandler<OnSelectedFloorGridChangedEventArgs> OnSelectedFloorGridChanged;
    public class OnSelectedFloorGridChangedEventArgs : EventArgs
    {
        public TrainFloorGrid selectedFloor;
    }
    public event EventHandler OnRemoveSelectedFloorGrid;

    [SerializeField] private GameObject buildingGhostSystemGameObject;
    [SerializeField] private LayerMask floorColliderLayerMask;
    
    private BuildingGhostSystem buildingGhostSystem;
    private bool isHoldingCabinet;
    private PlacedInteriorTypeSO placedInteriorTypeSO; // Maybe represent the cabinet player are selecting
    private TrainFloorGrid floorInteracted;
    private GameInput gameInput;
    private float placingDistance;
    private int cabinetHoldingCount = 0;
    
    public override void OnNetworkSpawn()
    {
        //InitializeFloorInteractedListServerRpc();

        if (IsOwner)
        {
            LocalInstance = this;
            
            SpawnBuildingGhostSystemServerRpc(OwnerClientId);
        }
        gameInput = GameInput.Instance;
        isHoldingCabinet = false;
        StartCoroutine(WaitAndGetBuildingGhostSystem());

        /*if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += Player_OnClientDisconnectCallback;
        }*/
    }

    private void Player_OnClientDisconnectCallback(ulong clientId) // STILL NEED TO FIX
    {
        Debug.Log("Player_OnClientDisconnectCallback");

        if (clientId == OwnerClientId && buildingGhostSystem != null)
        {
            /*NetworkObjectReference buildingGhostSystemNetworkRef = buildingGhostSystem.GetComponent<NetworkObject>();*/
            Debug.Log("buildingGhostSystem.NetworkObject: " + buildingGhostSystem.NetworkObject);

            NetworkObjectReference buildingGhostSystemNetworkRef = buildingGhostSystem.NetworkObject;
            Debug.Log("buildingGhostSystemNetworkRef: " + buildingGhostSystemNetworkRef);

            RemoveBuildingGhostSystemServerRpc(buildingGhostSystemNetworkRef);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveBuildingGhostSystemServerRpc(NetworkObjectReference buildingGhostSystemNetworkObjectRef)
    {
        Debug.Log("RemoveBuildingGhostSystemServerRpc");
        if(buildingGhostSystemNetworkObjectRef.TryGet(out NetworkObject buildingGhostSystemNetworkObject))
        {
            BuildingGhostSystem system = buildingGhostSystemNetworkObject.GetComponent<BuildingGhostSystem>();

            ClearKitchenObjectOnParentClientRpc();
            Destroy(system.gameObject);
        }
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc()
    {
        Debug.Log("ClearKitchenObjectOnParentClientRpc");

        buildingGhostSystem = null;
    }

    private IEnumerator WaitAndGetBuildingGhostSystem() // Maybe can switch to async and await (BAD CODE)
    {
        yield return new WaitForSeconds(0.1f); // Short delay to ensure the object is spawned and set as a child
        GetBuildingGhostSystemServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetBuildingGhostSystemServerRpc()
    {
        GetBuildingGhostSystemClientRpc();
    }

    [ClientRpc]
    private void GetBuildingGhostSystemClientRpc()
    {
        // Find the child that have BuildingGhostSystem and set buildingGhostSystem to that Getcomponent
        foreach (Transform child in transform)
        {
            BuildingGhostSystem foundSystem = child.GetComponent<BuildingGhostSystem>();
            if (foundSystem != null)
            {
                buildingGhostSystem = foundSystem;
                Debug.Log("BuildingGhostSystem assigned via search.");
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildingGhostSystemServerRpc(ulong clientId)
    {
        Debug.Log("SpawnBuildingGhostSystemServerRpc");

        // Instantiate the prefab and set ownership
        GameObject buildingGhostSystemObject = Instantiate(buildingGhostSystemGameObject);
        NetworkObject buidlingGhostSystemNetworkObj = buildingGhostSystemObject.GetComponent<NetworkObject>();
        //buidlingGhostSystemNetworkObj.Spawn(true);
        buidlingGhostSystemNetworkObj.SpawnWithOwnership(clientId);
        buidlingGhostSystemNetworkObj.transform.position = GetComponent<Player>().GetPlayerSpawnPositionList()[(int)OwnerClientId];
        buidlingGhostSystemNetworkObj.TrySetParent(GetComponent<NetworkObject>(), true);

    }

    private void Start()
    {
        gameInput.OnGrabAction += GameInput_OnGrabAction;
    }

    private void GameInput_OnGrabAction(object sender, EventArgs e)
    {
        if (!IsOwner)
        {
            return;
        }
        RecieveCabinetTemp();
    }

    private void OnTriggerEnter(Collider floor)
    {
        //TrainFloorGrid trainFloorGrid = floor.GetComponent<TrainFloorGrid>();
        
        if (floor.TryGetComponent(out TrainFloorGrid trainFloorGrid))
        {
            if (buildingGhostSystem == null)
            {
                StartCoroutine(WaitAndProcessTrigger(trainFloorGrid));
                return;
            }

            int floorIndex = TrainManager.Instance.GetTrainFloorGridIndex(trainFloorGrid);

            NetworkObjectReference buildingGhostRef = buildingGhostSystem.GetComponent<NetworkObject>();
            ChangedFloorInteractedLogicServerRpc(floorIndex, OwnerClientId, buildingGhostRef);

            Debug.Log("Entered TrainFloorGrid: " + trainFloorGrid.name);
        }
        else
        {
            Debug.Log("Something is triggering OnTriggerEnter");
        }
    }

    private IEnumerator WaitAndProcessTrigger(TrainFloorGrid trainFloorGrid) // Maybe can switch to async and await (BAD CODE)
    {
        yield return new WaitForSeconds(1f);

        if (buildingGhostSystem != null) 
        {
            int floorIndex = TrainManager.Instance.GetTrainFloorGridIndex(trainFloorGrid);
            NetworkObjectReference buildingGhostRef = buildingGhostSystem.GetComponent<NetworkObject>();
            ChangedFloorInteractedLogicServerRpc(floorIndex, OwnerClientId, buildingGhostRef);

            Debug.Log("Entered TrainFloorGrid after waiting: " + trainFloorGrid.name);
        }
        else
        {
            Debug.LogError("Failed to assign BuildingGhostSystem even after waiting.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TrainFloorGrid trainFloorGrid))
        {
            if (trainFloorGrid == TrainManager.Instance.GetFloorInteractedByClientIndex(OwnerClientId))
            {
                // Reset the reference when exiting the TrainFloorGrid
                NetworkObjectReference buildingGhostRef = buildingGhostSystem.GetComponent<NetworkObject>();
                RemoveFloorInteractedLogicServerRpc(OwnerClientId, buildingGhostRef);
                Debug.Log("Exited TrainFloorGrid: " + trainFloorGrid.name);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangedFloorInteractedLogicServerRpc(int floorIndex, ulong clientId, NetworkObjectReference buildingGhostRef)
    {
        ChangedFloorInteractedLogicClientRpc(floorIndex, clientId, buildingGhostRef);
    }

    [ClientRpc]
    private void ChangedFloorInteractedLogicClientRpc(int floorIndex, ulong clientId, NetworkObjectReference buildingGhostRef)
    {
        TrainFloorGrid floorGrid = TrainManager.Instance.GetTrainFloorGridByIndex(floorIndex);

        if (buildingGhostRef.TryGet(out NetworkObject buildingGhostNetworkObject))
        {
            buildingGhostNetworkObject.GetComponent<BuildingGhostSystem>().SelectedFloorGridChanged(clientId);
        }

        /*OnSelectedFloorGridChanged?.Invoke(this, new OnSelectedFloorGridChangedEventArgs
        {
            selectedFloor = floorGrid
        });
        Debug.LogWarning("OnTriggerEnter: " + (int)clientId);*/

        TrainManager.Instance.SetPlayerFloorInteractedByClientIndex(clientId, floorGrid);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveFloorInteractedLogicServerRpc(ulong clientId, NetworkObjectReference buildingGhostRef)
    {
        RemoveFloorInteractedLogicClientRpc(clientId, buildingGhostRef);
    }

    [ClientRpc]
    private void RemoveFloorInteractedLogicClientRpc(ulong clientId, NetworkObjectReference buildingGhostRef)
    {
        if (buildingGhostRef.TryGet(out NetworkObject buildingGhostNetworkObject))
        {
            buildingGhostNetworkObject.GetComponent<BuildingGhostSystem>().RemoveSelectedFloorGrid();

        }
        Debug.LogWarning("RemoveFloorInteractedLogicClientRpc");
        //OnRemoveSelectedFloorGrid?.Invoke(this, EventArgs.Empty);
        Debug.LogWarning("OnTriggerExit: " + (int)clientId);

        //floorInteractedArray[(int)clientId] = null;
        TrainManager.Instance.SetPlayerFloorInteractedByClientIndex(clientId, null);
    }

    public void HandlePlacingCabinet() 
    {
        if (cabinetHoldingCount == 0)
        {
            return;
        }
        Vector3 playerPointerPosition = GetPlacingCabinetVector();

        //floorInteracted = floorInteractedArray[(int)OwnerClientId];
        floorInteracted = TrainManager.Instance.GetFloorInteractedByClientIndex(OwnerClientId);

        if (floorInteracted != null)
        {
            //Debug.Log("Hit correct TrainFloorGrid on object: " + floorInteracted.name);
            if (isHoldingCabinet) 
            {
                if (floorInteracted.IsBuildable(playerPointerPosition, out Vector2Int placedObjectGridPosition))
                {
                    int placedCabinetTypeSOIndex = TrainManager.Instance.GetPlacedCabinetTypeSOIndex(placedInteriorTypeSO);

                    HandlePlacingCabinetLogicServerRpc(placedCabinetTypeSOIndex);

                    Debug.Log("floorInteracted: " + floorInteracted);

                    floorInteracted.PlaceDownInterior(placedObjectGridPosition, placedCabinetTypeSOIndex);
                }
                else // Not buildable
                {
                    Debug.Log("Cannot build here");
                }
            }
            
            else 
            {
                Debug.Log("isHoldingCabinet == false");
                //floorInteracted.PickUpCabinet(playerPointerPosition, GetComponent<Player>());
                //isHoldingCabinet = true;
            }
        }
        else
        {
            //Debug.Log("Hit an object, but it does not have TrainFloorGrid script.");
            return;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void HandlePlacingCabinetLogicServerRpc(int index)
    {
        HandlePlacingCabinetLogicClientRpc(index);
    }

    [ClientRpc]
    private void HandlePlacingCabinetLogicClientRpc(int index)
    {
        Debug.Log("HandlePlacingCabinetLogicClientRpc: " + cabinetHoldingCount);
        cabinetHoldingCount--;
        if (cabinetHoldingCount == 0)
        {
            Debug.Log("placedCabinetTypeSO and isHoldingCabinet should change to null and false");
            placedInteriorTypeSO = null;
            isHoldingCabinet = false;
            buildingGhostSystem.ResetSystem();
            KitchenObject kitchenObjectCrateHold = GetComponent<Player>().GetKitchenObject();
            if (kitchenObjectCrateHold != null) 
            {
                if (kitchenObjectCrateHold.TryGetComponent(out Crate crate))
                {
                    KitchenObject.DestroyKitchenObject(crate);
                }
                else
                {
                    Debug.Log("Cannot get componenet Crate");
                }
            }
            else
            {
                Debug.Log("Player pick up cabinet");
            }
        }
    }

    private void RecieveCabinetTemp() // TEMP
    {
        RecieveCabinetLogicServerRpc(0, 1, GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RecieveCabinetLogicServerRpc(int index, int cabinetIncrease, NetworkObjectReference playerNetworkObjectReference)
    {
        RecieveCabinetLogicClientRpc(index, cabinetIncrease);
        if (playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkRef))
        {
            PlayerPlacingCabinet playerPlacingCabinetTemp = playerNetworkRef.GetComponent<PlayerPlacingCabinet>();
            //playerPlacingCabinetTemp.SetPlacedCabinetTypeSO(placedCabinetTypeSOList[index]);
            //playerPlacingCabinetTemp.SetIsHoldingCabinet(true);
            Debug.Log("[Server]: ShowVisual()");
            BuildingGhostSystem buildingGhostSystemRef = playerPlacingCabinetTemp.GetBuildingGhostSystem();
            buildingGhostSystemRef.RefreshVisual(index, playerNetworkRef);
            buildingGhostSystemRef.ShowVisual();

        }
        else
        {
            Debug.LogError("Cannot get NetworkObject out of playerNetworkObjectReference");
        }
    }

    [ClientRpc]
    private void RecieveCabinetLogicClientRpc(int placedCabinetTypeSOIndex, int cabinetIncrease) // No idea why this work
    {
        //placedCabinetTypeSO = placedCabinetTypeSOList[placedCabinetTypeSOIndex];
        placedInteriorTypeSO =  TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);
        Debug.Log("placedCabinetTypeSO is " + placedInteriorTypeSO);
        isHoldingCabinet = true;
        cabinetHoldingCount += cabinetIncrease; // Need to check if match the placedCabinetTypeSO
    }
   
    public void RecieveCabinet(int cabinetIndex, int cabinetIncrease)
    {
        if (!IsServer)
        {
            Debug.Log("[ClientRpc]: "+ cabinetIndex);
        }
        NetworkObjectReference playerNetworkObjectReference = GetComponent<NetworkObject>();
        RecieveCabinetLogicServerRpc(cabinetIndex, cabinetIncrease, playerNetworkObjectReference);
    }

    public Vector3 GetPlacingCabinetVector()
    {
        placingDistance = Player.LocalInstance.GetPlacingDistance();
        Vector3 lastInterctDir = Player.LocalInstance.GetPlayerDirection();
        Vector3 placingPosition = transform.position + lastInterctDir * placingDistance;
        return placingPosition;
    }

    public void SetPlacedCabinetTypeSO(PlacedInteriorTypeSO cabinet)
    {
        placedInteriorTypeSO = cabinet;
    }

    public TrainFloorGrid GetTrainFloorInteracted()
    {
        return floorInteracted;
    }

    public PlacedInteriorTypeSO GetPlacedCabinetTypeSO()
    {
        return placedInteriorTypeSO;
    }

    public BuildingGhostSystem GetBuildingGhostSystem() { return buildingGhostSystem; }
    public void TogglePlacingCabinetSystem()
    {
        isHoldingCabinet = !isHoldingCabinet;
        Debug.Log("isPlacingCabinet is change to: " + isHoldingCabinet);
    }
    public void SetIsHoldingCabinet(bool Is)
    {
        isHoldingCabinet = Is;
    }

    public bool IsHoldingCabinet()
    {
        return isHoldingCabinet;
    }

    public void PickUpInterior(PlacedCabinet_Done placedCabinet_Done) 
    {
        // Get placedInteriorTypeSO
        if (placedCabinet_Done.TryGetComponent(out BaseCabinet baseCabinet))
        {
            placedInteriorTypeSO = baseCabinet.GetPlacedInteriorTypeSO();
        }
        else if (placedCabinet_Done.TryGetComponent(out BaseDesk baseDesk))
        {
            placedInteriorTypeSO = baseDesk.GetPlacedInteriorTypeSO();
        }
        else
        {
            Debug.LogWarning("Failed to TryGetComponent interface");
        }

        Debug.LogWarning("placedInteriorTypeSO: "+ placedInteriorTypeSO.name);

        // Recieve cabinet 
        NetworkObjectReference playerNetworkObjectReference = GetComponent<NetworkObject>();

        int placedCabinetTypeSOIndex = TrainManager.Instance.GetPlacedCabinetTypeSOIndex(placedInteriorTypeSO);

        RecieveCabinetLogicServerRpc(placedCabinetTypeSOIndex, 1, playerNetworkObjectReference);

        // Remove PlacedCabinet_Done
        Vector2Int placedInteriorOriginPosition = placedCabinet_Done.GetPlacedInteriorOriginPosition();
        RemoveInteriorLogicServerRpc(OwnerClientId, placedInteriorOriginPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveInteriorLogicServerRpc(ulong clientId, Vector2Int placedInteriorOriginPosition)
    {
        RemoveInteriorLogicClientRpc(clientId, placedInteriorOriginPosition);
    }

    [ClientRpc]
    private void RemoveInteriorLogicClientRpc(ulong clientId, Vector2Int placedInteriorOriginPosition)
    {
        TrainManager.Instance.GetFloorInteractedByClientIndex(clientId).RemoveInterior(placedInteriorOriginPosition);
    }

    public void SetBuildingGhostSystem(BuildingGhostSystem buildingGhostSystem)
    {
         this.buildingGhostSystem = buildingGhostSystem;
    }
    
    public bool IsInteractingWithGridFloor()
    {
        //return floorInteractedArray[(int)OwnerClientId] != null;
        return TrainManager.Instance.GetFloorInteractedByClientIndex(OwnerClientId) != null;
    }

}
