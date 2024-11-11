using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class DishWashCabinet : BaseCabinet
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    private int platesSpawnedAmount;
    private int platesSpawnedAmountMax = 4;

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!GameManager.Instance.IsTrainRunning())
        {
            return;
        }

        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;

            if (platesSpawnedAmount < platesSpawnedAmountMax)
            {
                SpawnPlateServerRpc();
            }

            //KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, this);
        }
    }

    public override void Interact(Player player)
    {
        //Check if there's at least 1 plate here
        if (platesSpawnedAmount > 0)
        {
            // Check if the player carrying something
            if (player.HasKitchenObject())
            {
                
            }
            // Player not carrying anything
            else
            {
                Debug.Log("Player took a plate");
                TakePlateServerRpc();
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            }
        }
        // There is no plate at all
        else
        {
            // Check if the player carrying something
            if (player.HasKitchenObject())
            {
                Debug.Log("There is no plate");
            }
            // Player not carrying anything
            else
            {
                //GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if (player.HasKitchenObject()) // Player maybe is holding a crate
        {
            Debug.Log("Player is holding a kitchenObject");
        }
        else if (player.gameObject.GetComponent<PlayerPlacingCabinet>().IsHoldingCabinet())
        {
            Debug.Log("Player is holding a cabinet");
        }
        else if (GameManager.Instance.IsTrainStop())
        {
            PlayerPlacingCabinet.LocalInstance.PickUpInterior(GetComponent<PlacedCabinet_Done>());
        }
        else
        {
            Debug.LogWarning("InteractAlternate nothing");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateServerRpc()
    {
        //Debug.Log("");
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        platesSpawnedAmount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakePlateServerRpc()
    {
        Debug.Log("InteractLogicServerRpc");
        TakePlateClientRpc();
    }


    [ClientRpc]
    private void TakePlateClientRpc()
    {
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
