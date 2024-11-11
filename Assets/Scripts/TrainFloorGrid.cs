using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCTCodeBase.Base;
using Unity.Netcode;
using static PlacedInteriorTypeSO;

public class TrainFloorGrid : NetworkBehaviour
{
    private PlacedInteriorTypeSO.Dir dir;

    private GridSystem<GridObject> grid;

    private Vector3 floorGridOriginPosition;
    private int gridWidth = 4;
    private int gridHeight = 2;
    private float cellSize = 2.5f;

    public override void OnNetworkSpawn()
    {
        // Server will handle the initialization of grid origin and inform clients
        InitializeGridOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeGridOnServerRpc()
    {
        // Attempt to retrieve the BoxCollider component
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider found on TrainFloorGrid!");
            return;
        }

        // Determine the grid's origin using the bounds of the BoxCollider
        Bounds combinedFloorBounds = boxCollider.bounds;
        floorGridOriginPosition = combinedFloorBounds.min;

        Debug.Log("[Server]: Calculated Grid Origin -" + floorGridOriginPosition);
        InitializeGridOnClientRpc(floorGridOriginPosition);
    }

    [ClientRpc]
    private void InitializeGridOnClientRpc(Vector3 floorPositionOrigin) 
    {
        if (!IsServer)
        {
            Debug.Log("[ClientRpc]: "+ floorPositionOrigin);
        }

        // Set the grid origin and initialize the grid system
        floorGridOriginPosition = floorPositionOrigin;
        grid = new GridSystem<GridObject>(gridWidth, gridHeight, cellSize, floorGridOriginPosition, (GridSystem<GridObject> g, int x, int y) => new GridObject(g, x, y));
    }

    public class GridObject
    {
        private GridSystem<GridObject> gridObject;
        private int x;
        private int y;
        public PlacedCabinet_Done placedInterior;

        public GridObject(GridSystem<GridObject> grid, int x, int y)
        {
            this.gridObject = grid;
            this.x = x;
            this.y = y;
            placedInterior = null;
        }

        public bool CanBuild()
        {
            return placedInterior == null;
        }

        public void SetPlacedCabinet(PlacedCabinet_Done placedCabinet)
        {
            this.placedInterior = placedCabinet;
            /*gridObject.TriggerGridCabinetChanged(x, y);*/
        }

        public PlacedCabinet_Done GetPlacedCabinet()
        {
            return placedInterior;
        }
        public void ClearPlacedInterior()
        {
            placedInterior = null;
            /*gridObject.TriggerGridObjectChanged(x, y);*/
        }
    }

    public void RemoveInterior(Vector2Int placedObjectOrigin)
    {
        // Get the cabinet in placedObjectOrigin position
        Vector3 placeObjectWorldPosition = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y);
        Debug.LogWarning("placeObjectWorldPosition");
        PlacedCabinet_Done placedCabinet = grid.GetGridObject(placeObjectWorldPosition, floorGridOriginPosition).GetPlacedCabinet();
        if (placedCabinet != null)
        {
            Debug.LogWarning("placedCabinet.DestroySelf()");

            placedCabinet.DestroySelf();
            grid.GetGridObject(placeObjectWorldPosition, floorGridOriginPosition).ClearPlacedInterior();
        }
    }

    public void PlaceDownInterior(Vector2Int placedObjectOrigin, int placedCabinetTypeSOIndex)
    {
        Vector3 placeObjectWorldPosition = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y);
        PlacedInteriorTypeSO placedCabinetTypeSO = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);
        Vector3 placedCabinetFloorPosition = grid.GetPlacedCabinetFloorGridPosition(placeObjectWorldPosition, placedCabinetTypeSO);
        if (placedCabinetTypeSO.InteriorType == PlacedInteriorTypeSO.Type.Cabinet)
        {
            bool isUpCabinet = grid.IsFacingUp(placeObjectWorldPosition);
            dir = placedCabinetTypeSO.SetRotationDirection(isUpCabinet);
            Quaternion Dir = Quaternion.Euler(0, placedCabinetTypeSO.GetRotationAngle(dir), 0);
            InstantiatePlacedCabinetServerRpc(placedCabinetTypeSOIndex, placedObjectOrigin, placedCabinetFloorPosition, Dir);
        }
        else if (placedCabinetTypeSO.InteriorType == PlacedInteriorTypeSO.Type.Table)
        {
            InstantiatePlacedTableServerRpc(placedCabinetTypeSOIndex, placedObjectOrigin, placedCabinetFloorPosition);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiatePlacedCabinetServerRpc(int placedCabinetTypeSOIndex, Vector2Int origin, Vector3 worldPosition, Quaternion Dir)
    {
        PlacedInteriorTypeSO placedCabinetTypeSO = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);
        Transform placedCabinetTransform = Instantiate(placedCabinetTypeSO.prefabCabinet, worldPosition, Dir);
        NetworkObject placedCabinetNetworkObject = placedCabinetTransform.GetComponent<NetworkObject>();
        placedCabinetNetworkObject.Spawn(true);
        NetworkObjectReference placedCabinetNetworkObjectRef = placedCabinetNetworkObject;
        SetUpPlacedCabinetLogicClientRpc(placedCabinetTypeSOIndex, origin, Dir, placedCabinetNetworkObjectRef);
    }

    [ClientRpc]
    private void SetUpPlacedCabinetLogicClientRpc(int placedCabinetTypeSOIndex, Vector2Int origin, Quaternion dir, NetworkObjectReference placedCabinetNetworkObjectRef)
    {
        PlacedInteriorTypeSO placedCabinetTypeSO = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);

        if (placedCabinetNetworkObjectRef.TryGet(out NetworkObject placedCabinetNetworkObject))
        {
            PlacedCabinet_Done placedCabinet_Done = placedCabinetNetworkObject.GetComponent<PlacedCabinet_Done>();
            grid.GetGridObject(origin.x, origin.y).SetPlacedCabinet(placedCabinet_Done);
            placedCabinet_Done.Setup(origin, dir, placedCabinetTypeSO);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiatePlacedTableServerRpc(int placedCabinetTypeSOIndex, Vector2Int origin, Vector3 worldPosition)
    {
        PlacedInteriorTypeSO placedCabinetTypeSO = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);
        Transform placedCabinetTransform = Instantiate(placedCabinetTypeSO.prefabCabinet, worldPosition, Quaternion.identity);
        NetworkObject placedCabinetNetworkObject = placedCabinetTransform.GetComponent<NetworkObject>();
        placedCabinetNetworkObject.Spawn(true);
        NetworkObjectReference placedCabinetNetworkObjectRef = placedCabinetNetworkObject;
        SetUpPlacedTableLogicClientRpc(placedCabinetTypeSOIndex, origin, placedCabinetNetworkObjectRef);
    }

    [ClientRpc]
    private void SetUpPlacedTableLogicClientRpc(int placedCabinetTypeSOIndex, Vector2Int origin, NetworkObjectReference placedCabinetNetworkObjectRef)
    {
        PlacedInteriorTypeSO placedCabinetTypeSO = TrainManager.Instance.GetPlacedCabinetTypeSOByIndex(placedCabinetTypeSOIndex);

        if (placedCabinetNetworkObjectRef.TryGet(out NetworkObject placedCabinetNetworkObject))
        {
            PlacedCabinet_Done placedCabinet_Done = placedCabinetNetworkObject.GetComponent<PlacedCabinet_Done>();
            grid.GetGridObject(origin.x, origin.y).SetPlacedCabinet(placedCabinet_Done);
            placedCabinet_Done.Setup(origin, placedCabinetTypeSO);
        }
    }

    public void PickUpCabinet(Vector3 playerPointerPosition, Player player)
    {
        // Valid Grid Position
        PlacedCabinet_Done placedCabinet = grid.GetGridObject(playerPointerPosition, /*this.transform.position*/ floorGridOriginPosition).GetPlacedCabinet();
        if (placedCabinet != null)
        {
            // Pick up cabinet
            placedCabinet.gameObject.GetComponent<BaseCabinet>().InteractAlternate(player);

            // Demolish
            placedCabinet.DestroySelf();
            grid.GetGridObject(playerPointerPosition, this.transform.position).ClearPlacedInterior();
        }
        else
        {
            Debug.Log("This gameObject player cannot pick up");
        }
    }

    public bool IsBuildable(Vector3 playerPointerPosition, out Vector2Int placedObjectGridPosition)
    {
        // Get the Cell by horizontal X and verticle Z 
        grid.GetCellXZByPosition(playerPointerPosition, /*transform.position*/ floorGridOriginPosition, out int x, out int z);

        // Store the XZ position Cell by Vector2Int
        Vector2Int placedObjectOrigin = new Vector2Int(x, z);

        // Make sure the Cell is a valid position in the grid 
        placedObjectOrigin = grid.ValidateGridPosition(placedObjectOrigin);

        Debug.Log("Can build: " + grid.GetGridObject(placedObjectOrigin.x, placedObjectOrigin.y).CanBuild());
        if (grid.GetGridObject(placedObjectOrigin.x, placedObjectOrigin.y).CanBuild())
        {
            placedObjectGridPosition = placedObjectOrigin;
            return true;
        }
        else
        {
            CodeBaseClass.CreateWorldTextPopup("Cannot Build Here!", new Vector3(placedObjectOrigin.x, 0, placedObjectOrigin.y));
            placedObjectGridPosition = placedObjectOrigin;
            return false;
        }
    }

    public Vector3 GetPlayerPointerSnappedPosition(PlacedInteriorTypeSO placedCabinetTypeSO)
    {
        Vector3 playerPointerPosition = PlayerPlacingCabinet.LocalInstance.GetPlacingCabinetVector();
        // Get the Cell by horizontal X and verticle Z 
        grid.GetCellXZByPosition(playerPointerPosition, floorGridOriginPosition, out int x, out int z);

        // Store the XZ position Cell by Vector2Int
        Vector2Int placedObjectOrigin = new Vector2Int(x, z);

        // Make sure the Cell is a valid position in the grid 
        placedObjectOrigin = grid.ValidateGridPosition(placedObjectOrigin);
        
        if (placedCabinetTypeSO != null && grid.GetGridObject(placedObjectOrigin.x, placedObjectOrigin.y).CanBuild())
        {
            Vector3 placeObjectWorldPosition = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y);

            // Change the cabinet position in the grid
            Vector3 placedCabinetFloorPosition = grid.GetPlacedCabinetFloorGridPosition(placeObjectWorldPosition, placedCabinetTypeSO);

            // Change the direction that cabinet is facing
            dir = placedCabinetTypeSO.SetRotationDirection(grid.IsFacingUp(placeObjectWorldPosition));

            return placedCabinetFloorPosition;
        }
        else
        {
            return playerPointerPosition;
        }
    }

    public Quaternion GetPlacedInteriorRotation(PlacedInteriorTypeSO placedInteriorTypeSO)
    {
        if (placedInteriorTypeSO != null && placedInteriorTypeSO.InteriorType == PlacedInteriorTypeSO.Type.Cabinet)
        {
            return Quaternion.Euler(0, placedInteriorTypeSO.GetRotationAngle(dir), 0);
        }
        else
        {
            return Quaternion.identity;
        }
    }
}
