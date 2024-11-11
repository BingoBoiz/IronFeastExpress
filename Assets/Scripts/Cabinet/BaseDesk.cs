using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseDesk : NetworkBehaviour
{
    public static event EventHandler OnAnyObjectPlacedHere;

    [SerializeField] private PlacedInteriorTypeSO placedInteriorTypeSO;
    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }
    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCabinet.Interact();");
    }
    public virtual void InteractAlternate(Player player)
    {
        Debug.LogError("BaseCabinet.InteractAlternate();");
    }
    public virtual void Grab(Player player)
    {
        Debug.LogError("BaseDesk.Grab();");
    }
    public PlacedInteriorTypeSO GetPlacedInteriorTypeSO()
    {
        return placedInteriorTypeSO;
    }
}
