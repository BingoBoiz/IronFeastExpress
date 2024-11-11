using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CabinetSetupSO", menuName = "Setup/CabinetSetup")]
public class CabinetSetupSO : ScriptableObject
{
    public List<CabinetSetup> cabinetSetups;
}

[System.Serializable]
public class CabinetSetup
{
    public PlacedInteriorTypeSO cabinetTypeSO; 
    public int trainFloorGridIndex;  
    public Vector2Int localGridPosition;    
}
