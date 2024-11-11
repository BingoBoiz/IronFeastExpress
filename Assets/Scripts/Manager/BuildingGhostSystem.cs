using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;


// Problem: When placed cabinet, cabinet still work and moving inside the grid slot when turn off
//

public class BuildingGhostSystem : NetworkBehaviour
{
    /*private PlayerPlacingCabinet player;*/
    private Transform cabinetVisual;
    private PlacedInteriorTypeSO placedCabinetTypeSO;
    private TrainFloorGrid floorShowCabinet;

    private NetworkVariable<Vector3> visualPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> visualRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Temp
    private float logCooldown = 0.5f; // Log every 0.5 seconds
    private float lastLogTime = 0f;

    public override void OnNetworkSpawn()
    {
        if (PlayerPlacingCabinet.LocalInstance != null)
        {
            //Debug.Log("PlayerPlacingCabinet.LocalInstance is not null, subscribing to events.");
            SubscribeToPlayerEvents();
        }
        else
        {
            //Debug.Log("PlayerPlacingCabinet.LocalInstance is null, waiting for initialization.");
            StartCoroutine(WaitForPlayerInstance());
        }
    }

    private IEnumerator WaitForPlayerInstance()
    {
        while (PlayerPlacingCabinet.LocalInstance == null)
        {
            yield return null; // Wait until the next frame
        }

        //Debug.Log("PlayerPlacingCabinet.LocalInstance initialized, subscribing to events.");
        SubscribeToPlayerEvents();
    }

    private void SubscribeToPlayerEvents()
    {
        PlayerPlacingCabinet.LocalInstance.OnSelectedFloorGridChanged -= Player_OnSelectedFloorGridChanged;
        PlayerPlacingCabinet.LocalInstance.OnRemoveSelectedFloorGrid -= Player_OnRemoveSelectedFloorGrid;

        PlayerPlacingCabinet.LocalInstance.OnSelectedFloorGridChanged += Player_OnSelectedFloorGridChanged;
        PlayerPlacingCabinet.LocalInstance.OnRemoveSelectedFloorGrid += Player_OnRemoveSelectedFloorGrid;
    }

    private void Player_OnRemoveSelectedFloorGrid(object sender, System.EventArgs e)
    {
        floorShowCabinet = null;
        HideVisual();
        Debug.Log("floorShowCabinet = null");
    }

    private void Player_OnSelectedFloorGridChanged(object sender, PlayerPlacingCabinet.OnSelectedFloorGridChangedEventArgs e)
    {
        floorShowCabinet = e.selectedFloor;
        ShowVisual();
        Debug.Log("floorShowCabinet: "+ floorShowCabinet);
    }

    public void RemoveSelectedFloorGrid()
    {
        floorShowCabinet = null;
        HideVisual();
        Debug.Log("floorShowCabinet = null");
    }

    public void SelectedFloorGridChanged(ulong ClientId)
    {
        floorShowCabinet = TrainManager.Instance.GetFloorInteractedByClientIndex(ClientId);
        ShowVisual();
        Debug.Log("floorShowCabinet: " + floorShowCabinet);
    }


    private void Update()
    {
        // Continuously sync the visual object with the network variables
        if (cabinetVisual != null)
        {
            // Use the NetworkVariable.Value directly
            cabinetVisual.position = Vector3.Lerp(cabinetVisual.position, visualPosition.Value, Time.deltaTime * 10f);
            cabinetVisual.rotation = Quaternion.Lerp(cabinetVisual.rotation, visualRotation.Value, Time.deltaTime * 10f);
            //DelayedSendDebugLog("cabinetVisual.position: " + cabinetVisual.position);
        }
    }

    private void LateUpdate()
    {
        //if (!IsSpawned || !IsOwner || cabinetVisual == null || !PlayerPlacingCabinet.LocalInstance.IsHoldingCabinet()) { return; }
        if (!IsSpawned)
        {
            return;
        }
        if (!IsOwner)
        {
            return;
        }
        if (cabinetVisual == null)
        {
            //DelayedSendDebugLog("cabinetVisual == null");
            return;
        }
        if (PlayerPlacingCabinet.LocalInstance == null)
        {
            DelayedSendDebugLog("PlayerPlacingCabinet.LocalInstance == null");
            return;
        }
        else if (!PlayerPlacingCabinet.LocalInstance.IsHoldingCabinet())
        {
            return;
        }
        else if (!PlayerPlacingCabinet.LocalInstance.IsInteractingWithGridFloor())
        {
            HideVisual();
            return;
        }

        if (GameManager.Instance.IsTrainStop())
        {
            if (placedCabinetTypeSO == null)
            {
                Debug.Log("placedCabinetTypeSO == null");
            }
            if (placedCabinetTypeSO != null && floorShowCabinet != null)
            {
                Vector3 targetPosition = floorShowCabinet.GetPlayerPointerSnappedPosition(placedCabinetTypeSO);
                //DelayedSendDebugLog("targetPosition: " + targetPosition);
                targetPosition.y = .2f;
                // Only send the new position and rotation to the server if there are changes
                if (Vector3.Distance(visualPosition.Value, targetPosition) > 0.01f)
                {
                    UpdatePositionOnServerRpc(targetPosition, floorShowCabinet.GetPlacedInteriorRotation(placedCabinetTypeSO));
                }
            }
            else
            {
                //HideVisual();
            }
        }
        else
        {
            HideVisual();
        }
    }

    private void DelayedSendDebugLog(string text)
    {
        // Log at a limited rate
        if (Time.time - lastLogTime >= logCooldown)
        {
            Debug.Log(text);
            lastLogTime = Time.time;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionOnServerRpc(Vector3 position, Quaternion rotation)
    {
        visualPosition.Value = position;
        visualRotation.Value = rotation;
    }

    public void RefreshVisual(int placedCabinetTypeSOIndex, NetworkObjectReference playerNetworkRef)
    {
        if (cabinetVisual != null) return; // If the visual already exists, don't create it again

        if (placedCabinetTypeSOIndex >= 0)
        {
            //Debug.Log("placedCabinetTypeSOIndex: " + placedCabinetTypeSOIndex);

            RefreshVisualLogicServerRpc(placedCabinetTypeSOIndex, playerNetworkRef);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RefreshVisualLogicServerRpc(int placedCabinetTypeSOIndex, NetworkObjectReference playerNetworkRef)
    {
        //Debug.Log("RefreshVisualLogicServerRpc: ");
        PlacedInteriorTypeSO placedCabinetTypeSOVar = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);
        
        if (playerNetworkRef.TryGet(out NetworkObject playerNetworkObject))
        {
            /*Debug.Log("playerNetworkObject " + playerNetworkObject);
            Debug.Log("buildingGhostSystemRef " + buildingGhostSystemRef);*/

            BuildingGhostSystem buildingGhostSystemRef = playerNetworkObject.GetComponent<PlayerPlacingCabinet>().GetBuildingGhostSystem();

            buildingGhostSystemRef.SetPlacedCabinetTypeSO(placedCabinetTypeSOVar);

            Transform cabinetVisual = Instantiate(placedCabinetTypeSOVar.placingVisual, Vector3.zero, Quaternion.identity);
            NetworkObject cabinetVisualNetworkObject = cabinetVisual.GetComponent<NetworkObject>();
            cabinetVisualNetworkObject.Spawn(true);

            cabinetVisualNetworkObject.TrySetParent(buildingGhostSystemRef.GetComponent<NetworkObject>(), true);

            // Create a reference to the barrel's NetworkObject
            NetworkObjectReference cabinetVisualNetworkObjectReference = cabinetVisualNetworkObject;
            NetworkObjectReference buildingGhostSystemNetworkObjectRef = buildingGhostSystemRef.NetworkObject;

            RefreshVisualLogicClientRpc(placedCabinetTypeSOIndex, cabinetVisualNetworkObjectReference, buildingGhostSystemNetworkObjectRef);
        }
        else
        {
            Debug.LogError("Cannot get NetworkObject out of playerNetworkRef");
        }
    }

    [ClientRpc]
    private void RefreshVisualLogicClientRpc(int placedCabinetTypeSOIndex, NetworkObjectReference cabinetVisualNetworkObjectReference, NetworkObjectReference buildingGhostSystemNetworkObjectRef)
    {
        //Debug.Log("RefreshVisualLogicClientRpc");
        if (cabinetVisualNetworkObjectReference.TryGet(out NetworkObject cabinetVisualNetworkObject))
        {
            cabinetVisual = cabinetVisualNetworkObject.transform; // Maybe
            cabinetVisual.localPosition = Vector3.zero;
            cabinetVisual.localEulerAngles = Vector3.zero;
            SetLayerRecursive(cabinetVisual.gameObject, 11);
        }
        else
        {
            Debug.LogError("Failed to get the NetworkObject from the reference!");
        }

        PlacedInteriorTypeSO placedCabinetTypeSOVar = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);

        if (buildingGhostSystemNetworkObjectRef.TryGet(out NetworkObject buildingGhostSystemNetworkObject))
        {
            buildingGhostSystemNetworkObject.GetComponent<BuildingGhostSystem>().SetPlacedCabinetTypeSO(placedCabinetTypeSOVar);
        }
    }

    public void ResetSystem()
    {
        HideVisual();
        NetworkObjectReference cabinetVisualRef = cabinetVisual.GetComponent<NetworkObject>();
        cabinetVisual = null;
        placedCabinetTypeSO = null;
        DestroyCabinetVisualServerRpc(cabinetVisualRef);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyCabinetVisualServerRpc(NetworkObjectReference cabinetVisualRef)
    {
        if (cabinetVisualRef.TryGet(out NetworkObject cabinetVisualNetworkObject))
        {
            Destroy(cabinetVisualNetworkObject);
        }
    }

    private void HideVisual()
    {
        if (cabinetVisual != null /*&& isShowing == true*/)
        {
            cabinetVisual.gameObject.SetActive(false);
            /*isShowing = false;*/
        }
    }

    public void ShowVisual()
    {
        Debug.Log("ShowVisual: " + cabinetVisual);
        if (cabinetVisual != null /*&& isShowing == false*/)
        {
            cabinetVisual.gameObject.SetActive(true);
            /*isShowing = true;*/
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer)
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
    private void SetPlacedCabinetTypeSO(PlacedInteriorTypeSO placedCabinetTypeSOVar)
    {
        placedCabinetTypeSO = placedCabinetTypeSOVar;
    }
}
