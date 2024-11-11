using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBoard : BaseDesk
{
    public static MenuBoard Instance { get; private set; }
    public event EventHandler OnInteractMenuBoard;

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    { 
        OnInteractMenuBoard?.Invoke(this, EventArgs.Empty);
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

}
