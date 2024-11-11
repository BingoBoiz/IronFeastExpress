using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Test : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            Debug.Log("[Test ClientRpc]: OnNetworkSpawn");
            
            return;
        }
        // Calculate combined bounds of all floor objects
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            Debug.LogError("No BoxCollider found on TrainFloorGrid!");
            return;
        }

        // Get the center of the combined bounds as the new origin for the grid
        Bounds combinedFloorBounds = boxCollider.bounds;
        Vector3 gridOrigin = combinedFloorBounds.min;
        TestServerRpc(gridOrigin);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestServerRpc(Vector3 gridOrigin)
    {
        Debug.Log("TestServerRpc");
        TestClientRpc(gridOrigin);
    }

    [ClientRpc]
    private void TestClientRpc(Vector3 gridOrigin)
    {
        Debug.Log("[TEST CLIENTRPC]: "+ gridOrigin);
    }
}
