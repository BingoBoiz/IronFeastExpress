using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class StoreInteriorSO : ScriptableObject
{
    public PlacedInteriorTypeSO placedCabinetTypeSO;
    public TrainFloorGrid trainFloorGrid;

    public Sprite storeImage;
    public float price;
    public float priceIncrease;
    public StoreType interiorType;
    public string storeInteriorName;

    public enum StoreType
    {
        Interior,
        Floor,
    }
}
