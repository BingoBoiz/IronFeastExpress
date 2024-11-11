using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStaticDataManager : MonoBehaviour
{
    private void Awake()
    {
        //CuttingCabinet.ResetStaticData();
        BaseCabinet.ResetStaticData();
        //TrashHole.ResetStaticData();
        Player.ResetStaticData();
    }
}
