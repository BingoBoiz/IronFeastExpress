using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreDesk : BaseDesk
{
    public static StoreDesk Instance { get; private set; }
    public static event EventHandler OnInteractStore;

    private bool isInteractingBySomePlayer = false; // Just to make sure there can only be maximum 1 player is interacting to this cabinet

    private void Awake()
    {
        Instance = this;
    }
    
    public override void Interact(Player player)
    {
        ToggleIsInteracting();
        if (isInteractingBySomePlayer)
        {
            OnInteractStore?.Invoke(this, EventArgs.Empty);
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

    public void ToggleIsInteracting() 
    {
        isInteractingBySomePlayer = !isInteractingBySomePlayer;
    }
}
