using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseSign : NetworkBehaviour
{
    //[SerializeField] private PlacedInteriorTypeSO placedCabinetTypeSO;
    
    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseSign.Interact();");
    }
    public virtual void InteractAlternate(Player player)
    {
        Debug.LogError("BaseSign.InteractAlternate();");
    }
}
