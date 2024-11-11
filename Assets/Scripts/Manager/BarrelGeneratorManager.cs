using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BarrelGeneratorManager : NetworkBehaviour
{
    public static BarrelGeneratorManager Instance {  get; private set; }   
    [SerializeField] private Transform barrelPrefab;
    [SerializeField] private Transform woodCratePrefab;
    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    
    private Transform barrelClone;
    private Transform crateClone;
    private Vector3 areaSize;
    private Vector3 areaCenter;

    private void Start()
    {
        Instance = this;

        StoreManager.Instance.OnBuyProduct += StoreManager_OnBuyProduct;
        StoreManager.Instance.OnBuyInteriorCabinet += StoreManager_OnBuyInterior;

        BoxCollider areaBarrelGenerate = GetComponent<BoxCollider>();
        if (areaBarrelGenerate == null)
        {
            Debug.LogError("Spawn Area does not have a BoxCollider component!");
            return;
        }

        // Calculate the area   
        Vector3 areaSize = areaBarrelGenerate.size;
        Vector3 areaCenter = areaBarrelGenerate.center;

    }

    private void StoreManager_OnBuyInterior(object sender, StoreManager.OnBuyInteriorCabinetEventArgs e)
    {
        for (int i = 0; i < e.storeInteriorSOList.Count; i++)
        {
            if (e.storeInteriorSOList[i].interiorType == StoreInteriorSO.StoreType.Interior)
            {
                GenerateWoodCrate(e.storeInteriorSOList[i], e.storeCabinetCountList[i]);
            }
            else
            {
                Debug.Log("storeInteriorSOList contains floor");
            }
        }
    }

    private void StoreManager_OnBuyProduct(object sender, StoreManager.OnBuyProductEventArgs e)
    {
        StartCoroutine(GenerateBarrelsWithDelay(e));
        /*for (int i = 0; i < e.storeProductSOList.Count; i++)
        {
            GenerateBarrels(e.storeProductSOList[i].kitchenObjectSO, e.storeProductCountList[i]);
        }*/
    }

    private void GenerateBarrels(KitchenObjectSO kitchenObjectSO, int kitchenObjectCount)
    {
        Debug.Log("Generate Barrels");
        int kitchenObjectSOIndex = kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);

        SpawnBarrelServerRpc(kitchenObjectSOIndex, kitchenObjectCount);
    }
    private IEnumerator GenerateBarrelsWithDelay(StoreManager.OnBuyProductEventArgs e)
    {
        float delayBetweenSpawns = 0.5f;
        for (int i = 0; i < e.storeProductSOList.Count; i++)
        {
            GenerateBarrels(e.storeProductSOList[i].kitchenObjectSO, e.storeProductCountList[i]);

            // Wait for a short delay between each spawn to avoid collisions
            yield return new WaitForSeconds(delayBetweenSpawns); // Adjust delay as needed (0.5 seconds here)
        }
    }

    private void GenerateWoodCrate(StoreInteriorSO storeInteriorSO, int cabinetCount)
    {
        Debug.Log("Generate Wood Crate");
        int placedCabinetIndex = StoreManager.Instance.GetStoreInteriorSOIndex(storeInteriorSO);
        SpawnWoodCrateServerRpc(placedCabinetIndex, cabinetCount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBarrelServerRpc(int kitchenObjectSOIndex, int kitchenObjectCount)
    {
        Debug.Log("SpawnBarrelServerRpc");

        GetRandomVector(out Vector3 randomPosition, out Quaternion randomRotation);

        barrelClone = Instantiate(barrelPrefab, randomPosition, randomRotation);

        NetworkObject barrelNetworkObject = barrelClone.GetComponent<NetworkObject>();
        barrelNetworkObject.Spawn(true);

        // Create a reference to the barrel's NetworkObject
        NetworkObjectReference barrelNetworkObjectReference = barrelNetworkObject;

        SpawnBarrelClientRpc(kitchenObjectSOIndex, kitchenObjectCount, barrelNetworkObjectReference);
    }

    [ClientRpc]
    private void SpawnBarrelClientRpc(int kitchenObjectSOIndex, int kitchenObjectCount, NetworkObjectReference barrelNetworkObjectReference)
    {
        Debug.Log("SpawnBarrelClientRpc");

        if (barrelNetworkObjectReference.TryGet(out NetworkObject barrelNetworkObject))
        {
            // Get the barrel component from the NetworkObject
            Barrel barrel = barrelNetworkObject.GetComponent<Barrel>();

            // Get the KitchenObjectSO using the index
            KitchenObjectSO kitchenObjectSO = kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];

            // Update the barrel's kitchen object and count
            barrel.SetKitchenObjectSO(kitchenObjectSO);
            barrel.SetKitchenObjectCount(kitchenObjectCount);
        }
        else
        {
            Debug.LogError("Failed to get the NetworkObject from the reference!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnWoodCrateServerRpc(int storeInteriorSO, int cabinetCount)
    {
        GetRandomVector(out Vector3 randomPosition, out Quaternion randomRotation);

        crateClone = Instantiate(woodCratePrefab, randomPosition, randomRotation);

        NetworkObject crateNetworkObject = crateClone.GetComponent<NetworkObject>();
        crateNetworkObject.Spawn(true);

        // Create a reference to the barrel's NetworkObject
        NetworkObjectReference barrelNetworkObjectReference = crateNetworkObject;

        SpawnWoodCrateClientRpc(storeInteriorSO, cabinetCount, barrelNetworkObjectReference);
    }

    [ClientRpc]
    private void SpawnWoodCrateClientRpc(int cabinetIndex, int CabinetCount, NetworkObjectReference crateNetworkObjectReference)
    {
        if (crateNetworkObjectReference.TryGet(out NetworkObject crateNetworkObject))
        {
            // Get the Crate component from the NetworkObject
            Crate crate = crateNetworkObject.GetComponent<Crate>();

            // Get the placeCabinetTypeSO using the index
            StoreInteriorSO storeInteriorSO = StoreManager.Instance.GetStoreInteriorSOByIndex(cabinetIndex);
            PlacedInteriorTypeSO placedCabinetTypeSO = storeInteriorSO.placedCabinetTypeSO;

            // Update the barrel's kitchen object and count
            crate.SetPlacedCabinetTypeSO(placedCabinetTypeSO);
            crate.SetCabinetCount(CabinetCount);
        }
        else
        {
            Debug.LogError("Failed to get the NetworkObject from the reference!");
        }
    }

    private void GetRandomVector(out Vector3 randomPosition, out Quaternion randomRotation)
    {
        // Generate a random position in spawn zone
        randomPosition = new Vector3(
                Random.Range(areaCenter.x - areaSize.x / 2, areaCenter.x + areaSize.x / 2),
                Random.Range(areaCenter.x - areaSize.y / 2, areaCenter.x + areaSize.y / 2),
                Random.Range(areaCenter.z - areaSize.z / 2, areaCenter.z + areaSize.z / 2)
            ) + this.transform.position;
        Debug.Log(randomPosition);

        // Create random rotation
        randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        Debug.Log(randomRotation);
    }

}
