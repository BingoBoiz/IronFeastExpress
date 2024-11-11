using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlacedCabinet_Done : NetworkBehaviour
{
    private PlacedCabinet_Done placedCabinet_Done;
    private PlacedInteriorTypeSO placedCabinetTypeSO;
    private Vector2Int origin;
    private Quaternion dir;
    
    public void Setup(Vector2Int origin, Quaternion dir, PlacedInteriorTypeSO placedCabinetTypeSO)
    {
        this.placedCabinetTypeSO = placedCabinetTypeSO;
        this.origin = origin;
        this.dir = dir;
    }

    public void Setup(Vector2Int origin, PlacedInteriorTypeSO placedCabinetTypeSO)
    {
        this.placedCabinetTypeSO = placedCabinetTypeSO;
        this.origin = origin;
        this.dir = Quaternion.identity;
    }

    public Vector2Int GetPlacedInteriorOriginPosition()
    {
        return origin;
    }

    /*public PlacedCabinet_Done GetPlacedCabinet_Done()
    {
        return placedCabinet_Done;
    }*/

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
