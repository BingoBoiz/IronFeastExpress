using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using System;
using TMPro;

public class TrainManager : NetworkBehaviour
{
    public static TrainManager Instance {  get; private set; }
    public event EventHandler OnDoneSetUp; // Bad way to tell the UI when to listen to other event
    public event EventHandler OnAddingNewWagon;
    [SerializeField] private GameObject customerWagonPrefab;
    [SerializeField] private GameObject kitchenWagonPrefab;
    [SerializeField] private CabinetSetupSO cabinetSetupSO;
    [SerializeField] private float wagonSpacing = 1.0f; // Offset for the position between wagons
    [SerializeField] private List<PlacedInteriorTypeSO> placedCabinetTypeSOList;

    // A list to keep track of all train wagons
    private List<GameObject> wagons = new List<GameObject>();
    private List<TrainFloorGrid> floorList = new List<TrainFloorGrid>();
    private GameObject newWagon; // just for adding new wagon
    private bool isMovingAirdropWagon = false;

    
    
    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        // Initialize train with default wagons
        InitializeTrain();
        InitializeFloorList();
        
        if (IsServer) 
        {
            StartCoroutine(WaitForPlayerPlacingCabinet());
        }
        StoreManager.Instance.OnBuyInteriorCabinet += StoreManager_OnBuyInterior;
    }

    private IEnumerator WaitForPlayerPlacingCabinet() // BAD CODE
    {
        yield return new WaitForSeconds(1f);
        InitialSetupCabinets();
    }

    private void InitialSetupCabinets()
    {
        //Debug.Log("InitialSetupCabinets");
        if (cabinetSetupSO == null || cabinetSetupSO.cabinetSetups.Count == 0)
        {
            Debug.Log("No cabinet setup data found.");
            return;
        }

        foreach (CabinetSetup cabinet in cabinetSetupSO.cabinetSetups)
        {
            if (cabinet.trainFloorGridIndex < 0 || cabinet.trainFloorGridIndex >= floorList.Count)
            {
                Debug.Log("Invalid floor grid index in cabinet setup: " + cabinet.trainFloorGridIndex);
                continue;
            }
            //Debug.Log("cabinet: " + cabinet.cabinetTypeSO.name);
            int placedCabinetTypeSOIndex = GetPlacedCabinetTypeSOIndex(cabinet.cabinetTypeSO);
            PlaceCabinetOnGridServerRpc(placedCabinetTypeSOIndex, cabinet.trainFloorGridIndex, cabinet.localGridPosition);
        }
        AnnounceDoneSetUpServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceCabinetOnGridServerRpc(int placedCabinetTypeSOIndex, int floorGridIndex, Vector2Int gridPosition)
    {
        Debug.Log("PlaceCabinetOnGridServerRpc (" + placedCabinetTypeSOIndex + ", " + floorGridIndex + ", " + gridPosition + ") ");

        if (floorGridIndex < 0 || floorGridIndex >= floorList.Count)
        {
            Debug.Log($"Invalid floor grid index: {floorGridIndex}");
            return;
        }

        TrainFloorGrid targetFloorGrid = floorList[floorGridIndex];

        // Place cabinet on the specified grid position
        targetFloorGrid.PlaceDownInterior(gridPosition, placedCabinetTypeSOIndex); // Adjust as needed
    }

    [ServerRpc(RequireOwnership = false)]
    private void AnnounceDoneSetUpServerRpc() { AnnounceDoneSetUpClientRpc(); }

    [ClientRpc]
    private void AnnounceDoneSetUpClientRpc() { OnDoneSetUp?.Invoke(this, EventArgs.Empty); }

    private void StoreManager_OnBuyInterior(object sender, StoreManager.OnBuyInteriorCabinetEventArgs e)
    {
        for (int i = 0; i < e.storeInteriorSOList.Count; i++)
        {
            if (e.storeInteriorSOList[i].interiorType == StoreInteriorSO.StoreType.Floor)
            {
                int storeInteriorSOIndex = StoreManager.Instance.GetStoreInteriorSOIndex(e.storeInteriorSOList[i]);
                AddWagon(storeInteriorSOIndex, e.storeCabinetCountList[i]);
            }
            else
            {
                Debug.Log("storeInteriorSOList contains cabinet");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddWagon(0, 1);
        }
    }

    // Method to initialize the train with default wagons
    private void InitializeTrain()
    {
        wagons.Add(transform.Find("SteamLocomotive").gameObject);
        wagons.Add(transform.Find("CustomerWagon").gameObject);
        wagons.Add(transform.Find("KitchenWagon").gameObject);
        wagons.Add(transform.Find("AirdropWagon").gameObject);
    }

    private void InitializeFloorList()
    {
        foreach (GameObject wagon in wagons)
        {
            if(wagon.TryGetComponent<TrainFloorGrid>(out TrainFloorGrid floor))
            {
                floorList.Add(floor);
            }
        }
    }

    private Vector3 CalculateWagonSize(GameObject wagon)
    {
        Renderer[] renderers = wagon.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogError($"No renderers found for wagon: {wagon.name}");
            return Vector3.zero;
        }

        // Initialize bounds using the first renderer's bounds
        Bounds totalBounds = renderers[0].bounds;

        // Expand the bounds to include all renderers
        foreach (Renderer renderer in renderers)
        {
            totalBounds.Encapsulate(renderer.bounds);
        }

        // Return the size of the combined bounds
        return totalBounds.size;
    }

    public void AddWagon(int storeInteriorSOIndex, int wagonCount)
    {
        for (int i =0; i < wagonCount; i++)
        {
            AddWagonLogicServerRpc(storeInteriorSOIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddWagonLogicServerRpc(int storeInteriorSOIndex)
    {
        // Calculate the position for the new wagon
        Vector3 newPosition = CalculateNextWagonPosition();

        StartCoroutine(MoveAirdropWagonAndAddNewWagon(newPosition, storeInteriorSOIndex));
    }

    private IEnumerator MoveAirdropWagonAndAddNewWagon(Vector3 newPosition, int storeInteriorSOIndex)
    {
        MoveAirdropWagonClientRpc(newPosition);

        yield return new WaitUntil(() => !isMovingAirdropWagon); 

        InstantiateNewWagon(storeInteriorSOIndex, newPosition);
    }

    private void InstantiateNewWagon(int storeInteriorSOIndex, Vector3 newPosition)
    {
        StoreInteriorSO storeInteriorSO = StoreManager.Instance.GetStoreInteriorSOByIndex(storeInteriorSOIndex);

        newWagon = Instantiate(storeInteriorSO.trainFloorGrid.gameObject, newPosition, transform.rotation, this.transform);
        NetworkObject wagonNetworkObject = newWagon.GetComponent<NetworkObject>();
        wagonNetworkObject.Spawn(true);

        wagonNetworkObject.TrySetParent(GetComponent<NetworkObject>(), true);
        AddWagonLogicClientRpc(wagonNetworkObject);
    }

    [ClientRpc]
    private void AddWagonLogicClientRpc(NetworkObjectReference wagonNetworkObjectReference)
    {
        if (wagonNetworkObjectReference.TryGet(out NetworkObject wagonNetworkObject))
        {
            TrainFloorGrid newFloor = wagonNetworkObject.GetComponent<TrainFloorGrid>();
            // Add the new wagon to the list, inserting it before the Airdrop Wagon
            wagons.Insert(wagons.Count - 1, wagonNetworkObjectReference);
            floorList.Add(newFloor);
            OnAddingNewWagon?.Invoke(this, EventArgs.Empty);
            // Recalculate the position of the Airdrop Cargo to be after the new wagon
            //MoveAirdropWagon();
        }
    }

    private Vector3 CalculateNextWagonPosition()
    {
        if (wagons.Count == 0)
        {
            // If no wagons yet, start at the origin
            return transform.position;
        }

        // Get the last wagon in the list (before the Airdrop Wagon)
        GameObject lastWagon = wagons[wagons.Count - 2]; // -2 to avoid Airdrop Wagon
        Vector3 lastWagonPosition = lastWagon.transform.position;

        // Calculate the new position based on the size of the last wagon
        Vector3 lastWagonSize = CalculateWagonSize(lastWagon);
        return lastWagonPosition - transform.right * (lastWagonSize.x + wagonSpacing); // Adjusted to follow TrainManager's rotation
    }

    [ClientRpc]
    private void MoveAirdropWagonClientRpc(Vector3 lastWagonPosition)
    {
        // Get the new last wagon before the Airdrop Wagon
        GameObject lastWagon = wagons[wagons.Count - 2]; // -2 to get the wagon before Airdrop

        // Get the Airdrop Wagon, which is the last wagon in the list
        GameObject airdropWagon = wagons[wagons.Count - 1];

        // Store the previous position of the Airdrop Wagon
        Vector3 previousPosition = airdropWagon.transform.position;

        // Calculate the new position for the Airdrop Wagon
        Vector3 newPosition = lastWagonPosition - transform.right * (CalculateWagonSize(lastWagon).x + wagonSpacing);

        StartCoroutine(MoveWagonAndContentsSmoothly(airdropWagon, newPosition));

        BarrelGeneratorManager.Instance.transform.position += newPosition - previousPosition;
    }

    private IEnumerator MoveWagonAndContentsSmoothly(GameObject airdropWagon, Vector3 targetPosition)
    {
        isMovingAirdropWagon = true;
        float lerpDuration = 1.0f;  // Adjust for desired speed
        float elapsedTime = 0;

        Vector3 startPosition = airdropWagon.transform.position;
        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
           
            airdropWagon.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        // Ensure final positions are exact
        airdropWagon.transform.position = targetPosition;
        isMovingAirdropWagon = false;
    }

    public int GetTrainFloorGridIndex(TrainFloorGrid floor)
    {
        return floorList.IndexOf(floor);
    }

    public TrainFloorGrid GetTrainFloorGridByIndex(int index)
    {
        return floorList[index];
    }
    public int GetPlacedCabinetTypeSOIndex(PlacedInteriorTypeSO cabinetTypeSO)
    {
        return placedCabinetTypeSOList.IndexOf(cabinetTypeSO);
    }

    public PlacedInteriorTypeSO GetPlacedCabinetTypeSOByIndex(int cabinetTypeSOIndex)
    {
        return placedCabinetTypeSOList[cabinetTypeSOIndex];
    }

    private int[] playerFloorInteractedIndexArray = new int[4]; // 4 Max Player

    public TrainFloorGrid GetFloorInteractedByClientIndex(ulong clientId)
    {
        int floorIndex = playerFloorInteractedIndexArray[(int)clientId];
        if (floorIndex == -1)
        {
            return null;
        }
        return floorList[floorIndex];

    }

    public void SetPlayerFloorInteractedByClientIndex(ulong clientId, TrainFloorGrid trainFloorGrid)
    {
        if (trainFloorGrid != null)
        {
            playerFloorInteractedIndexArray[(int)clientId] = floorList.IndexOf(trainFloorGrid);
        }
        else
        {
            playerFloorInteractedIndexArray[(int)clientId] = -1;
        }
    }
}
