using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewspaperHolder : BaseDesk
{
    public static NewspaperHolder Instance { get; private set; }

    public event EventHandler OnInteract;

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    {
        OnInteract?.Invoke(this, EventArgs.Empty);
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
