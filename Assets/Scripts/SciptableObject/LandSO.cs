using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Land", menuName = "ScriptableObjects/LandSO")]
public class LandSO : ScriptableObject
{
    public string landName;

    [TextArea(5, 10)]
    public string landDescription;

    public List<CustomerSO> customers;
}
