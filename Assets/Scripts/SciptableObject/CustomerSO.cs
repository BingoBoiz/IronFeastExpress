using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CustomerSO : ScriptableObject
{
    public string customerID;
    public string customerName;
    public string customerOccupation;
    public List<FinishDishSO> favoriteDish;
    public List <FinishDishSO> hateDish;
    public string customerDescription;
    public Transform customerPrefab;
}
