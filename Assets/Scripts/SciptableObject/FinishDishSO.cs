using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FinishDishSO : ScriptableObject
{
    public List<KitchenObjectSO> ingridientKitchenObjectSOList;
    public string finishDishName;
    public Sprite finishDishIcon;
    public float finishDishPrice;
}
